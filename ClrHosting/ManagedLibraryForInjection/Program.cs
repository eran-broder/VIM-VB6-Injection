using Brotils;
using ManagedLibraryForInjection.IPC;
using ManagedLibraryForInjection.VB;
using Newtonsoft.Json;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO.Pipes;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using SharedStructures;
using Win32Utils;

namespace ManagedLibraryForInjection
{
    public class Program
    {
        private static MainThreadDispatcher _dispatcher;
        private static Thread _workerThread;
        private static CancellationTokenSource _cancellationTokenSource;
        private static IntPtr _hookHandle;

        static void Main(string[] args)
        {
            var msg = "Main is not supposed to be called at all";
            Console.WriteLine(msg);
            throw new Exception(msg);
        }

        public static int InvokePendingMessage(IntPtr handle)
        {
            Console.WriteLine($"Managed code requested to handle message {handle}");
            _dispatcher.Invoke(handle.ToInt32());
            return 222;
        }

        public static int Shutdown(IntPtr _)
        {
            Console.WriteLine("C# canceling token");
            _cancellationTokenSource.Cancel();
            _workerThread.Join(); //How long should I wait? make it configurable
            Console.WriteLine("C# worker thread is dead");
            return 0;
        }

        //TODO: this is strange. why here? what is this class? it does way too much
        public static void FreeHookedLibrary()
        {
            //TODO: why do you know the name? it should be passed as a parameter!
            var hmod = PInvoke.LoadLibrary("called.dll");
            PInvoke.FreeLibrary(hmod);
        }


        //DO NOT MAKE ASYNC. I DO NOT KNOW THE REPERCUSSIONS
        //BUT on the other hand - the code is shit this way. think it over
        public static int DoWork(ArgumentsForManagedLibrary args)
        {
            Console.WriteLine($"Invoked [{MethodBase.GetCurrentMethod().DeclaringType}]");
            _workerThread = new Thread(_ =>
            {
                try
                {
                    //WorkTaskDelegate(handle).Wait();
                    var pipe = CreatePipe();
                    HookWindow(args.PathOfInjectedDll, args.WindowToHook);
                    StateWaitingForConnection(pipe, args.WindowToHook).Wait();
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                }
            });
            _workerThread.Start();
            return 1;
        }

        private static void HookWindow(string pathOfInjectedDll, IntPtr handleOfWindow)
        {

            var threadId = PInvoke.GetWindowThreadProcessId(handleOfWindow, out _);
            Assersions.Assert(threadId != 0, $"Error getting thread for handle [{handleOfWindow}]");
            var dll = PInvoke.LoadLibrary(pathOfInjectedDll);
            Assersions.NotNull(dll , $"Cannot load dll from {pathOfInjectedDll}");
            var addressAsIntPtr = PInvoke.GetProcAddress(dll, "GetMsgProc");
            var addressAsDelegate = Marshal.GetDelegateForFunctionPointer<PInvoke.HookProc>(addressAsIntPtr);
            Assersions.NotNull(addressAsIntPtr , $"Cannot find function in dll to hook");
            _hookHandle = PInvoke.SetWindowsHookEx(PInvoke.HookType.WH_GETMESSAGE, addressAsDelegate, dll, threadId);
            Assersions.NotNull(_hookHandle , $"{nameof(PInvoke.SetWindowsHookEx)} failed");
        }

        private static CancellationTokenSource GetCancellationToken()
        {
            return new CancellationTokenSource();
            
        }

        
        //TODO: this should return both the task and the cancellation token
        //TODO: this is not good. there is an implicit state here. a state-machine is required 
        //TODO: perhaps some of the code can be invoked using the IPC?
        /*private static async Task WorkTaskDelegate(IntPtr handle)
        {
            var token = GetCancellationToken();//TODO: check what happens if canceled during bootstrap
            var pipe = CreatePipe();
            await pipe.WaitForConnectionAsync(token);
            Console.WriteLine("Client connected"); //TODO: statemachine it
        }*/

        private static async Task StateWaitingForConnection(NamedPipeServerStream pipe, IntPtr handle)
        {
            var token = GetCancellationToken();//TODO: check what happens if canceled during bootstrap
            await pipe.WaitForConnectionAsync(token.Token);
            await StateConsumeMessages(pipe, handle, token);
        }

        private static async Task StateConsumeMessages(NamedPipeServerStream pipe, IntPtr handle, CancellationTokenSource tokenSource)
        {
            var token = tokenSource.Token;
            var (write, read) = CreateSyncedTunnel();

            var readerTask = Task.Run(() => StreamWrapper.ReadLoop(pipe, write, token), token);

            async Task WriteToPipe(string message)
            {
                var messageAsByteArray = Encoding.Default.GetBytes(message);//TODO: are you sure about default?
                await pipe.WriteAsync(messageAsByteArray, token);
            }

            var consumerTask = Task.Run(() => ConsumeMessages(handle, read, WriteToPipe, token), token);
            var allTasks = new[] {readerTask, consumerTask};
            await Task.WhenAny(allTasks);//TODO: this is a mistake. I need to explicitly identify client disconnect
            tokenSource.Cancel();
            pipe.Close();
            await pipe.DisposeAsync();
            await StateWaitingForConnection(CreatePipe(), handle);
        }

        private static (Action<string, CancellationToken> put, Func<CancellationToken, string> get) CreateSyncedTunnel()
        {
            var queue = new BlockingCollection<string>();
            return ((s, token) => queue.Add(s, token), (token) => queue.Take(token));
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

        private static async Task ConsumeMessages(IntPtr handle, Func<CancellationToken, string> blockingMessageProvider, Func<string, Task> pipeWrite, CancellationToken token)
        {
            var handlers = CreateMessageHandlers(handle);
            var messageHandler = new MessageHandlerCollection(handlers);
            while (!token.IsCancellationRequested)
            {
                var rawMessage = blockingMessageProvider.Invoke(token);
                Console.WriteLine($"Got a message {rawMessage}");
                await messageHandler.Digest(rawMessage).Map(
            task => {
                        Console.WriteLine($"Got back : [{task.Result.Value}]");
                        pipeWrite(JsonConvert.SerializeObject(task.Result));  //TODO: should I await this?
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
                {"VB", new VbMessageHandler(func => _dispatcher.Dispatch(func))},
                {"Internal", new ReflectionBasedHandler(typeof(Program))} //TODO: DRY? no real alternatives
            };
            return handlers;
        }

        public void Unload()
        {
            _cancellationTokenSource.Cancel();

        }

        
    }
}
