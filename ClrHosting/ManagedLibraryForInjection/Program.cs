using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.IO.Pipes;
using System.Reflection.Metadata.Ecma335;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using Brotils;
using ManagedLibraryForInjection.IPC;
using ManagedLibraryForInjection.VB;
using Newtonsoft.Json;
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


        //DO NOT MAKE ASYNC. I DO NOT KNOW THE REPERCUSSIONS
        public static int DoWork(IntPtr handle)
        {
            var pipeWrite = StartPipe().Result;
            StartHandlerTask(handle, pipeWrite);
            return 333;
        }

        private static Task StartHandlerTask(IntPtr handle, Action<string> pipeWrite)
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

                    //TODO: there is a context problem here. who is waiting for the task? no one. no thread? think it over
                    await response.Map(
                        task =>
                        {
                            Console.WriteLine($"Got back : [{task.Result.Value}]");
                            pipeWrite(JsonConvert.SerializeObject(task.Result)); //TODO: make it async!
                        },
                        task => Console.WriteLine($"Could not resolve message : [{task.Status}] [{task.Exception?.Message}]")
                    );
                }
            });


        }
        private static Dictionary<string, IMessageHandler> CreateMessageHandlers(IntPtr handle)
        {
            _dispatcher = new MainThreadDispatcher(handle, 1031);

            var handlers = new Dictionary<string, IMessageHandler>
            {
                {"VB", new VbMessageHandler(func => _dispatcher.Dispatch(func))}
            };
            return handlers;
        }

        //TODO: return task
        private static Task<Action<string>> StartPipe()
        {
            Console.WriteLine($"Starting pipe on thread : [{Thread.CurrentThread.ManagedThreadId}]");
            var pipeName = $"VimEmbedded";//_{Process.GetCurrentProcess().Id}";
            Console.WriteLine($"listening on: {pipeName} on thread [{Thread.CurrentThread.ManagedThreadId}]");
            NamedPipeServerStream stream =
                new NamedPipeServerStream(pipeName, PipeDirection.InOut, 1, PipeTransmissionMode.Message);

            return stream.WaitForConnectionAsync().Success<Action<string>>(_ =>
            {
                Console.WriteLine("Client connected!");
                StreamWrapper.Create(stream).AddHandler(s => _queue.Add(s)).Start();
                StreamWriter writer = new StreamWriter(stream);
                return msg => writer.Write(msg);
            });


        }
    }
}
