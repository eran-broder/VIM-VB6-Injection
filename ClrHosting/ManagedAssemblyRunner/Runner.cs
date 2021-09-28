using System;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Runtime.Loader;
using System.Threading.Tasks;
using Brotils;
using SharedStructures;

namespace ManagedAssemblyRunner
{
    public class Runner: IContextManager
    {
        private Type _type;
        private static readonly Runner _instance = new Runner();
        private AssemblyLoadContext _context;

        public static int DoWork(ref AssemblyRunnerInfo info)//IntPtr infoPtr)
        {
            return _instance.DoWorkInstance(ref info);
        }

        private int DoWorkInstance(ref AssemblyRunnerInfo info)
        {
            var clone = Clone(info);
            _context = new AssemblyLoadContext("mainContext", true);
            Console.WriteLine($"Path : {info.assemblyPath}");
            var assembly = _context.LoadFromAssemblyPath(info.assemblyPath);
            Console.WriteLine($"Loading assembly : {assembly}");
            _type = assembly.GetType(info.nameOfClass);
            Assersions.NotNull(_type, $"type failed loading \"{info.nameOfClass}\".");
            _type.GetMethod(info.methodName)
                .Invoke(null, new object?[] { info.ArgumentsForManagedLibrary, this});

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
            var method = _instance._type.GetMethod("InvokePendingMessage");
            var parameters = new object?[1];
            parameters[0] = handle;
            method.Invoke(null, parameters);
            return 0;
        }

        public void UnloadCurrent()
        {

            Console.WriteLine("@Unloading context@");
            _context.Unload();            
        }
    }
}
