using System;
using System.Reflection;
using System.Runtime.Loader;

namespace ManagedAssemblyRunner
{
    public static class Runner
    {
        public static int DoWork(IntPtr handle)
        {
            Console.WriteLine("Runner called @@@@@@@@@@@@@@@@!");
            var context = new AssemblyLoadContext("mainContext", true);
            Console.WriteLine("Before load");
            var assemblyName = new AssemblyName("ManagedLibraryForInjection, Version=1.0.0.0");
            //var assembly = context.LoadFromAssemblyName(assemblyName); //TODO: by name or by path?
            var assembly = context.LoadFromAssemblyPath()
            Console.WriteLine(assembly);
            var type = assembly.GetType("ManagedLibraryForInjection.Program");
            Console.WriteLine(type);
            Console.WriteLine("Calling");
            type.GetMethod("DoWork")
                .Invoke(null, new object?[]{IntPtr.Zero});
            
            return 0;
        }
    }
}
