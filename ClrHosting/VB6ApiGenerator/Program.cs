using System;
using System.IO;

namespace VB6ApiGenerator
{
    class Program
    {
        interface IEcwMethods
        {
            bool SetReferral(int id);
        }

        static void Main(string[] args)
        {
            var methods = typeof(IEcwMethods).GetMethods();
            var code = Generator.Generate(methods);
            var path =
                @"C:\Users\broder\Documents\GitHub\VIM-VB6-Injection\ClrHosting\ManagedLibraryForInjection\NativeDllWrapper.cs";

            File.WriteAllText(path, code);

        }
    }

    
}
