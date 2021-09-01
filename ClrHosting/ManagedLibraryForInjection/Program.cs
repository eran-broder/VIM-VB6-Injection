using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.IO.Pipes;
using System.Reflection.Metadata.Ecma335;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Brotils;
using ManagedLibraryForInjection.IPC;
using ManagedLibraryForInjection.VB;
using Newtonsoft.Json;
using Win32Utils;

namespace ManagedLibraryForInjection
{
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


        //DO NOT MAKE ASYNC. I DO NOT KNOW THE REPERCUSSIONS
        //BUT on the other hand - the code is shit this way. think it over
        public static int DoWork(IntPtr handle)
        {
            var thread = new Thread(_ => WorkTaskDelegate(handle));
            thread.Start();
            return 1;
        }

        private static async Task WorkTaskDelegate(IntPtr handle)
        {
            var pipe = CreatePipe();
            await pipe.WaitForConnectionAsync(); //TODO: use a cancellation token?
            var (write, read) = CreateSyncedTunnel();
            var streamReader = StreamWrapper.Create(pipe).AddHandler(write).Start();

            async Task WriteToPipe(string message)
            {
                var messageAsByteArray = Encoding.Default.GetBytes(message);//TODO: are you sure about default?
                await pipe.WriteAsync(messageAsByteArray);
            }

            //TODO: do you really need a task here?
            await Task.Run(() => ConsumeMessages(handle, read, WriteToPipe));
        }


        private static (Action<string> put, Func<string> get) CreateSyncedTunnel()
        {
            var queue = new BlockingCollection<string>();
            return (s => queue.Add(s), () => queue.Take());
        }

        private static NamedPipeServerStream CreatePipe()
        {
            Console.WriteLine($"Starting pipe on thread : [{Thread.CurrentThread.ManagedThreadId}]");
            var pipeName = $"VimEmbedded";//_{Process.GetCurrentProcess().Id}";
            Console.WriteLine($"listening on: {pipeName} on thread [{Thread.CurrentThread.ManagedThreadId}]");
            NamedPipeServerStream stream =
                new NamedPipeServerStream(pipeName, PipeDirection.InOut, 1, PipeTransmissionMode.Message, PipeOptions.Asynchronous);
            return stream;
        }

        private static async Task ConsumeMessages(IntPtr handle, Func<string> blockingMessageProvider, Func<string, Task> pipeWrite)
        {
            var handlers = CreateMessageHandlers(handle);
            var messageHandler = new MessageHandlerCollection(handlers);
            while (true) //TODO: until when?
            {
                var rawMessage = blockingMessageProvider.Invoke();
                var response = messageHandler.Digest(rawMessage);
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
    }
}
