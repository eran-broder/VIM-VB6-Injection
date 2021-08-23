using System;
using System.Linq;
using System.Threading;
using ManagedLibraryForInjection;

namespace NetWrapperForTesting
{
    class Program
    {
        static void Main(string[] args)
        {
            ManagedLibraryForInjection.Program.DoWork(IntPtr.Zero);
            while (true)
            {
                Console.Write(".");
                Thread.Sleep(TimeSpan.FromSeconds(1.5));
            }
        }
    }

}
