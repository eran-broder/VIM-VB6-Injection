using System;
using System.CodeDom.Compiler;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Reflection.Emit;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using Brotils;
using Microsoft.CSharp;
using Win32Utils;

namespace ManagedLibraryForInjection.VB
{
    record VbMessage(string FunctionName, object[] Parameters);
    class LibraryInvoker : MessageHandlerBase<VbMessage>, IDisposable
    {
        private readonly Func<Func<object>, Task<object>> _invoker;
        private IntPtr _hmod;

        public static LibraryInvoker Create(Func<Func<object>, Task<object>> invoker, string libraryPath)
        {
            var hmod = PInvoke.LoadLibrary(libraryPath);
            Assersions.NotNull(hmod, $"Cannot load library [{libraryPath}]");
            return new LibraryInvoker(invoker, hmod);
        }
        private LibraryInvoker(Func<Func<object>, Task<object>> invoker, IntPtr hmod)
        {
            _invoker = invoker;
            _hmod = hmod;
        }
        protected override Task<object> HandleMessage(VbMessage message)
        {
            var functionToRun = GetFunction(message);
            return _invoker(() => functionToRun());

        }

        private Func<object> GetFunction(VbMessage message)
        {
            var @delegate = CreateLibraryDelegate(message);
            return () => @delegate.DynamicInvoke(message.Parameters);
        }

        private Delegate CreateLibraryDelegate(VbMessage message)
        {
            var argumentsTypes = message.Parameters.Select(p => p.GetType());
            var procAddress = PInvoke.GetProcAddress(_hmod, message.FunctionName);
            Assersions.NotNull(procAddress, $"Cannot find proc named [{message.FunctionName}]");
            var methodType = DelegateCreator.NewDelegateType(typeof(int), argumentsTypes.ToArray());
            //var methodType = typeof(PredefinedDelegates.None);
            var @delegate = Marshal.GetDelegateForFunctionPointer(procAddress, methodType);
            return @delegate;
        }

        public static class DelegateCreator
        {
            //this is a bomb waiting to blow. I did not find any other way
            private static readonly Func<Type[], Type> MakeNewCustomDelegate = 
                (Func<Type[], Type>)Delegate.CreateDelegate(typeof(Func<Type[], Type>), typeof(Expression).Assembly.GetType("System.Linq.Expressions.Compiler.DelegateHelpers").GetMethod("MakeNewCustomDelegate", BindingFlags.NonPublic | BindingFlags.Static));

            public static Type NewDelegateType(Type ret, params Type[] parameters)
            {
                var argsAndReturn = parameters.Concat(new[] {ret}).ToArray();
                return MakeNewCustomDelegate(argsAndReturn);
            }
        }

        public void Dispose()
        {
            PInvoke.FreeLibrary(_hmod);
        }

        //TODO: not functional. I do not like it at all
        public void ReloadLibrary(string newPath)
        {
            PInvoke.FreeLibrary(_hmod);
            _hmod = PInvoke.LoadLibrary(newPath);
        }
    }
}