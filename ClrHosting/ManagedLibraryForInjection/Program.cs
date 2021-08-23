using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Pipes;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using Brotils;
using ManagedLibraryForInjection.IPC;
using Win32Utils;

namespace ManagedLibraryForInjection
{

    public delegate long ExportedFunction();
    public delegate void SetReferral();
    

    public class Program
    {
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


        
        public static int DoWork(IntPtr handle)
        {
            Console.WriteLine("Yheaaaa baby!!!");
            PInvoke.PostMessage(handle, 1030, 0, IntPtr.Zero);

            var handlers = new Dictionary<string, IMessageHandler>
            {
                {"VB", new VbMessageHandler()}
            };
            var messageHandler = new MessageHandlerCollection(handlers);
            var activeProvider = NamePipeTransportAsEventProvider.Strat("VimEmbedded")
                .AddHandler(s =>
                {
                    Console.WriteLine($"Got message : [{s}]");
                    var response = messageHandler.Digest(s);
                    response.Success(t => Console.WriteLine(t.Result));
                })
                .Start();

            
            return 333;
        }

    }
}
