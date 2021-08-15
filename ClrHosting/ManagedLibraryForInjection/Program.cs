using System;

namespace ManagedLibraryForInjection
{
    class Program
    {
        static void Main(string[] args)
        {
            var msg = "Main is not supposed to be called at all";
            Console.WriteLine(msg);
            throw new Exception(msg);
        }

        public static int DoWork(int x)
        {
            Console.WriteLine($"Embedded code is executed with [{x}]");
            return x * 2;
        }
    }
}
