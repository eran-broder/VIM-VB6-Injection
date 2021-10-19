using Brotils;
using ManagedLibraryForInjection.IPC;
using ManagedLibraryForInjection.VB;
using Newtonsoft.Json;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
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
        private static IContextManager _contextManager;
        private static IntPtr _hookHandle;
        private static LibraryInvoker _libraryInvoker;

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

        //DO NOT MAKE ASYNC. I DO NOT KNOW THE REPERCUSSIONS
        public static int DoWork(ArgumentsForManagedLibrary args, IContextManager contextManager)
        {
            _contextManager = contextManager;
            _workerThread = new Thread(_ =>
            {
                try
                {
                    var pipe = CreatePipe();
                    Console.WriteLine($"HANDLE [{args.WindowToHook}]");
                    _hookHandle = HookWindow(args.PathOfInjectedDll, args.WindowToHook);
                    StateWaitingForConnection(pipe, args.WindowToHook).Map(
                        task => { },
                        task => Console.WriteLine($"main task failed : [{task.Exception}]")).Wait(); //TODO: what should we do?
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                }
            });
            _workerThread.Start();
            return 1;
        }

        private static IntPtr HookWindow(string pathOfInjectedDll, IntPtr handleOfWindow)
        {
            Console.WriteLine($"Hooking from : {pathOfInjectedDll}");
            var threadId = PInvoke.GetWindowThreadProcessId(handleOfWindow, out _);
            Assersions.Assert(threadId != 0, $"Error getting thread for handle [{handleOfWindow}]");
            var dll = PInvoke.LoadLibrary(pathOfInjectedDll);
            Assersions.NotNull(dll , $"Cannot load dll from {pathOfInjectedDll}");
            var addressAsIntPtr = PInvoke.GetProcAddress(dll, "GetMsgProc");
            var addressAsDelegate = Marshal.GetDelegateForFunctionPointer<PInvoke.HookProc>(addressAsIntPtr);
            Assersions.NotNull(addressAsIntPtr , $"Cannot find function in dll to hook");
            var hookHandle = PInvoke.SetWindowsHookEx(PInvoke.HookType.WH_GETMESSAGE, addressAsDelegate, dll, threadId);
            Assersions.NotNull(hookHandle, $"{nameof(PInvoke.SetWindowsHookEx)} failed");
            return hookHandle;
        }

        private static async Task StateWaitingForConnection(NamedPipeServerStream pipe, IntPtr handle)
        {
            _cancellationTokenSource = new CancellationTokenSource();
            Console.WriteLine("Awaiting connection");
            await pipe.WaitForConnectionAsync(_cancellationTokenSource.Token);
            Console.WriteLine("Connected");
            await StateConsumeMessages(pipe, handle, _cancellationTokenSource);
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

            var handlers = CreateMessageHandlers(handle);

            var consumerTask = Task.Run(() => ConsumeMessages(handlers, read, WriteToPipe, token), token);

            await readerTask.Success(async task =>
            {
                //TODO: await the consumer task as well?
                Console.WriteLine("Client disconnected");
                tokenSource.Cancel();
                pipe.Close();
                await pipe.DisposeAsync();
                await StateWaitingForConnection(CreatePipe(), handle);
            });
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

        private static async Task ConsumeMessages(IReadOnlyDictionary<string, IMessageHandler> handlers, Func<CancellationToken, string> blockingMessageProvider, Func<string, Task> pipeWrite, CancellationToken token)
        {
            while (!token.IsCancellationRequested)
            {
                Console.WriteLine("Awaiting message queue");
                var rawMessage = blockingMessageProvider.Invoke(token);
                Console.WriteLine($"Got a message {rawMessage}");
                await MessageDigester.Digest(rawMessage, handlers).Map(async task => {
                        Console.WriteLine($"Got back : [{task.Result.Value}]");
                        await pipeWrite(JsonConvert.SerializeObject(task.Result));
                   },
                    task => Console.WriteLine($"Could not resolve message : [{task.Status}] [{task.Exception?.Message}]")
                );
            }
        }
        private static Dictionary<string, IMessageHandler> CreateMessageHandlers(IntPtr handle)
        {
            //TODO: what is this shit? no way!!!
            _dispatcher = new MainThreadDispatcher(handle, 1031);
            _libraryInvoker = LibraryInvoker.Create(func => _dispatcher.Dispatch(func), "called.dll");

            var handlers = new Dictionary<string, IMessageHandler>
            {
                //{"VB", new VbMessageHandler(func => _dispatcher.Dispatch(func))},
                {"VB", _libraryInvoker},
                {"Internal", new ReflectionBasedHandler(typeof(Program))}
            };
            return handlers;
        }

        public static void ReloadVb(string dllPath)
        {
            string tempDirectory = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());
            Directory.CreateDirectory(tempDirectory);
            var name = Path.GetFileName(dllPath);
            var copyPath = Path.Combine(tempDirectory, name);
            File.Copy(dllPath, copyPath);
            //_libraryInvoker.Dispose();
            _libraryInvoker.ReloadLibrary(copyPath);
        }

    }
}
