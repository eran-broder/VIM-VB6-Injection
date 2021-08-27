using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.IO.Pipes;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using Brotils;
using ManagedLibraryForInjection.IPC;
using ManagedLibraryForInjection.VB;
using Win32Utils;

namespace ManagedLibraryForInjection
{

    public delegate long ExportedFunction();
    public delegate void SetReferral();
    

    public class Program
    {
        private static MainThreadDispatcher _dispatcher;
        private static BlockingCollection<string> _queue;

        static void Main(string[] args)
        {
            var msg = "Main is not supposed to be called at all";
            Console.WriteLine(msg);
            throw new Exception(msg);
        }

        public static int InvokeFromMainThread(IntPtr handle)
        {
            EcwEmbeddedAdapterProxy.SetReferral();
            return 222;
        }


        public static int InvokePendingMessage(IntPtr handle)
        {
            _dispatcher.Invoke(handle.ToInt32());
            return 222;
        }


        public static int DoWork(IntPtr handle)
        {
            Console.WriteLine($"DO Work on thread : [{Thread.CurrentThread.ManagedThreadId}]");
            PInvoke.PostMessage(handle, 1030, 0, IntPtr.Zero);
            StartPipe();
            StartHandlerTask(handle);
            return 333;
        }

        private static Task StartHandlerTask(IntPtr handle)
        {
            return Task.Run(async () =>
            {
                var handlers = CreateMessageHandlers(handle);
                var messageHandler = new MessageHandlerCollection(handlers);
                _queue = new BlockingCollection<string>();
                while (true)
                {
                    var s = _queue.Take();

                    var response = messageHandler.Digest(s);
                    DUMMY_GLOBAL.TheTask = response;

                    
                    Console.WriteLine(response);
                    //TODO: there is a context problem here. who is waiting for the task? no one. no thread? think it over
                    await response
                        .Success(t => Console.WriteLine($"!!!!!!!!!!!!!SUCCESS!!!!!!!!!!!!!"))
                        .Error(task => Console.WriteLine($"*ERROR* : {task.Exception?.Message}"))
                        .Canceled(task =>
                        {

                            Console.WriteLine($"WTF???? why canceled??? [{Thread.CurrentThread.ManagedThreadId}]");
                        });
                }
            });


        }

        class DummyVbHandler: MessageHandlerBase<VbMessage>
        {
            protected override Task<object> HandleMessage(VbMessage message)
            {
                return Task.FromResult("DUMMY RESULT" as object);
            }
        }

        private static Dictionary<string, IMessageHandler> CreateMessageHandlers(IntPtr handle)
        {
            _dispatcher = new MainThreadDispatcher(handle, 1031);

            var handlers = new Dictionary<string, IMessageHandler>
            {
                //{"VB", new VbMessageHandler(func => _dispatcher.Dispatch(func))}
                {"VB", new DummyVbHandler()}
            };
            return handlers;
        }

        private static void StartPipe()
        {
            Console.WriteLine($"Starting pipe on thread : [{Thread.CurrentThread.ManagedThreadId}]");
            var activeProvider = NamePipeTransportAsEventProvider.Strat("VimEmbedded")
                .AddHandler(s =>
                {
                    Console.WriteLine($"Got message : [{s}]");
                    _queue.Add(s);
                }).Start();
        }
    }
}
