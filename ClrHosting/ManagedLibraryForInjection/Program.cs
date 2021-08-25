using System;
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
            Console.WriteLine("Yheaaaa baby!!!");
            PInvoke.PostMessage(handle, 1030, 0, IntPtr.Zero);

            _dispatcher = new MainThreadDispatcher(handle, 1031);
            var handlers = new Dictionary<string, IMessageHandler>
            {
                {"VB", new VbMessageHandler(func => _dispatcher.Dispatch(func))}
            };
            var messageHandler = new MessageHandlerCollection(handlers);
            var activeProvider = NamePipeTransportAsEventProvider.Strat("VimEmbedded")
                .AddHandler(s =>
                {
                    Console.WriteLine($"Got message : [{s}]");
                    var response = messageHandler.Digest(s);

                    //TODO: there is a context problem here. who is waiting for the task? no one. no thread.
                    response
                        .Success(t => Console.WriteLine($"Response for:\r\n\t[{s}]\r\n\t[{t.Result}]"))
                        .Error(task => Console.WriteLine($"*ERROR* : {task.Exception?.Message}"))
                        .Canceled(task => Console.WriteLine("WTF???? why canceled???")).Wait();
                })
                .Start();

            
            return 333;
        }

    }
}
