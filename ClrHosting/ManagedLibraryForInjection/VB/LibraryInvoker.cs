using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using Brotils;
using Optional.Collections;
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
            var procAddress = PInvoke.GetProcAddress(_hmod, message.FunctionName);
            Assersions.NotNull(procAddress, $"Cannot find proc named [{message.FunctionName}]");
            var adapted = message.Parameters.Select(AdaptArgument).ToArray();
            var adaptedValues = adapted.Select(a => a.adapted).ToArray();
            var adaptedTypes = adaptedValues.Select(a => a.GetType()).ToArray();
            var cleanup = CombineActions(adapted.Select(a => a.cleanup));
            
            //TODO: explicitly specify the return value in the message? yes!
            var methodType = DelegateCreator.NewDelegateType(typeof(int), adaptedTypes);
            var @delegate = Marshal.GetDelegateForFunctionPointer(procAddress, methodType);
            return () =>
            {
                var result = @delegate.DynamicInvoke(adaptedValues);
                cleanup();
                return result;
            }; 
        }

        private Action CombineActions(IEnumerable<Action> actions) => () => { foreach (var action in actions) action(); };


        private Dictionary<Type, Func<object, (object result, Action cleanup)>> _typeMap =
            new()
            {
                {typeof(string), o =>
                {
                    var bstr = Marshal.StringToBSTR((string) o);
                    void Free() => Marshal.FreeBSTR(bstr);
                    return (bstr, Free);
                }} 
            };

        private (object adapted, Action cleanup) AdaptArgument(object argument)
        {
            return _typeMap.GetValueOrNone(argument.GetType()).Match(func => func(argument), () => (argument, () => { }));
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