using System;
using System.IO;
using System.IO.Pipes;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using Win32Utils;

namespace ManagedLibraryForInjection
{

    public delegate long ExportedFunction();

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
            Console.WriteLine($"~~~~~~~~~~~~~~~~~Thread [{PInvoke.GetCurrentThreadId()}]~~~~~~~~~~~~~~~~~~");
            return 500;
        }

        public static int DoWork(IntPtr handle)
        {
            LoadCodeFromVB();
            Win32Utils.PInvoke.PostMessage(handle, 1030, 0, IntPtr.Zero);
            //SetupTask(handle);
            return 333;
        }

        private static void LoadCodeFromVB()
        {
            Console.WriteLine("HELLO!!!!!!!");
            var vbHandle = PInvoke.LoadLibrary("called.dll");
            Console.WriteLine($"address of dll: {vbHandle}");
            var addressAsIntPtr = PInvoke.GetProcAddress(vbHandle, "ExportedFunction");
            Console.WriteLine($"address from VB: {addressAsIntPtr}");
            var addressAsDelegate = Marshal.GetDelegateForFunctionPointer<ExportedFunction>(addressAsIntPtr);
            var result = addressAsDelegate();
            Console.WriteLine($"From dll = {result}");
        }

        private static void SetupTask(IntPtr handle)
        {
            try
            {
                Task.Run(PipeServerTask).Wait();
            }
            catch (Exception e)
            {
                Console.WriteLine("ERROR: " + e.Message);
            }
        }

        private static async Task PipeServerTask()
        {
            try
            {

                NamedPipeServerStream pipeServer =
                    new NamedPipeServerStream("testpipe",
                        PipeDirection.InOut,
                        1,
                        PipeTransmissionMode.Message);

                Console.WriteLine("waiting for connection");
                //TODO: use cancellation token?
                await pipeServer.WaitForConnectionAsync();
                Console.WriteLine("Client connected");
                var reader = new StreamReader(pipeServer);
                while (true)
                {
                    var data = await reader.ReadLineAsync();
                    Console.WriteLine($"Got back from read [{data?.Length}]");
                    if (!string.IsNullOrEmpty(data))
                        Console.WriteLine(data);
                }
            }
            catch (Exception e)
            {
                
            }


        }
    }
}
