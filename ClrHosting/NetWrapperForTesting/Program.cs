using System;
using System.Threading;

namespace NetWrapperForTesting
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Hello World!");
            ManagedLibraryForInjection.Program.DoWork(IntPtr.Zero);
            Console.ReadLine();
        }
    }
}
