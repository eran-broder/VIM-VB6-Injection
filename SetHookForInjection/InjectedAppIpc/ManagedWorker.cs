using System;
using System.ComponentModel;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using JetBrains.Annotations;

namespace ManagedLibrary
{
    public class ManagedWorker
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Hello World!");
        }

        public delegate int ReportProgressFunction(int progress);

        // This test method doesn't actually do anything, it just takes some input parameters,
        // waits (in a loop) for a bit, invoking the callback function periodically, and
        // then returns a string version of the double[] passed in.
        [return: MarshalAs(UnmanagedType.LPStr)]
        public static string DoWork(
            [MarshalAs(UnmanagedType.LPStr)] string jobName,
            int iterations,
            int dataSize,
            [MarshalAs(UnmanagedType.LPArray, SizeParamIndex = 2)] double[] data,
            ReportProgressFunction reportProgressFunction)
        {
            for (int i = 1; i <= iterations; i++)
            {
                Console.Beep(1300, 100);
                Console.ForegroundColor = ConsoleColor.Cyan;
                Console.WriteLine($"Beginning work iteration {i}");
                Console.ResetColor();

                // Pause as if doing work
                Thread.Sleep(1000);

                // Call the native callback and write its return value to the console
                var progressResponse = reportProgressFunction(i);
                Console.WriteLine($"Received response [{progressResponse}] from progress function");
            }


            return $"Data received: {string.Join(", ", data.Select(d => d.ToString()))}";
        }

    
        [UsedImplicitly]
        public static int DoWork2(
            [MarshalAs(UnmanagedType.LPStr)] string jobName)
        {
            //Console.WriteLine($"CALLLLED with " + jobName);
            //Task.Run(() => GetAction(handleOfHook));
            //Task.Run(GetAction);
            return 987;
        }

        private static void GetAction()
        {
            while (true)
            {
                Console.Beep();
                //PostMessage(new IntPtr(handleOfHook), 1030, new IntPtr(333), new IntPtr(444));
                Thread.Sleep(TimeSpan.FromSeconds(2));
            }
        }


        //[return: MarshalAs(UnmanagedType.Bool)]
        //[DllImport("user32.dll", SetLastError = true, CharSet = CharSet.Auto)]
        //static extern bool PostMessage(IntPtr hWnd, uint Msg, IntPtr wParam, IntPtr lParam);

        
        private static void GetAction2(int handleOfHook)
        {
            while (true)
            {
                Console.Beep();
                //PostMessage(new IntPtr(handleOfHook), 1030, new IntPtr(333), new IntPtr(444));
                Thread.Sleep(TimeSpan.FromSeconds(2));
            }
        }
    }



}
