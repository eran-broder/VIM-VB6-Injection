using System;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Runtime.Loader;
using System.Threading.Tasks;
using Brotils;
using SharedStructures;

namespace ManagedAssemblyRunner
{
    public static class Runner
    {
        private static Type _type;

        public static int DoWork(ref AssemblyRunnerInfo info)//IntPtr infoPtr)
        {
            var clone = Clone(info);
            var context = new AssemblyLoadContext("mainContext", true);
            Console.WriteLine($"Path : {info.assemblyPath}");
            var assembly = context.LoadFromAssemblyPath(info.assemblyPath);
            Console.WriteLine($"Loading assembly : {assembly}");
            _type = assembly.GetType(info.nameOfClass);
            Assersions.Assert(_type != null, $"type failed loading \"{info.nameOfClass}\".");
            _type.GetMethod(info.methodName)
                .Invoke(null, new object?[]{info.ArgumentsForManagedLibrary});

            return 0;
        }

        private static AssemblyRunnerInfo Clone(AssemblyRunnerInfo info)
        {
            return new AssemblyRunnerInfo()
            {
                methodName = info.methodName,
                nameOfClass = info.nameOfClass,
                assemblyPath = info.assemblyPath,
                ArgumentsForManagedLibrary = new ArgumentsForManagedLibrary(){WindowToHook = info.ArgumentsForManagedLibrary.WindowToHook}
            };
        }

        public static int InvokePendingMessage(IntPtr handle)
        {
            var method = _type.GetMethod("InvokePendingMessage");
            var parameters = new object?[1];
            parameters[0] = handle;
            method.Invoke(null, parameters);
            return 0;
        }
    }
}
