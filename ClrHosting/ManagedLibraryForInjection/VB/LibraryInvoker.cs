using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using Brotils;
using Optional;
using Optional.Collections;
using Win32Utils;
using DefaultReturnType = System.IntPtr;
namespace ManagedLibraryForInjection.VB
{
    record VbMessage(string FunctionName, object[] Parameters, object ReturnValue = null);
    //TODO: this class has parts that are pure and parts that are specific. split it.
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

        //TODO: bad piece of code here. this funciton is hard to read
        private Func<object> GetFunction(VbMessage message)
        {
            var procAddress = PInvoke.GetProcAddress(_hmod, message.FunctionName);
            Assersions.NotNull(procAddress, $"Cannot find proc named [{message.FunctionName}]");
            var adapted = message.Parameters.Select(AdaptArgument).ToArray();
            var adaptedValues = adapted.Select(a => a.adapted).ToArray();
            var adaptedTypes = adaptedValues.Select(a => a.GetType()).ToArray();
            var cleanupArguments = CombineActions(adapted.Select(a => a.cleanup));
            
            //TODO: explicitly specify the return value in the message? yes!
            var methodType = DelegateCreator.NewDelegateType(typeof(DefaultReturnType), adaptedTypes);
            var @delegate = Marshal.GetDelegateForFunctionPointer(procAddress, methodType);
            return () =>
            {
                //TODO: this is a very implicit convert. remove it?
                var result = (DefaultReturnType)(@delegate.DynamicInvoke(adaptedValues));
                cleanupArguments();
                return message.ReturnValue
                    .SomeNotNull()
                    .Map(v => _returnTypeMap[v.GetType()])
                    .Match(
                    func =>
                    {
                        var (modifiedResult, cleanupManagedResult) = func(result);
                        cleanupManagedResult();//TODO: this is strange. perhaps encapsulate withing the modifier itself?
                        return modifiedResult;

                    },
                    () => result);
            }; 
        }

        private Action CombineActions(IEnumerable<Action> actions) => () => { foreach (var action in actions) action(); };


        //TODO: do you want to change the return type of the delegate as well accordingly?
        private readonly Dictionary<Type, Func<DefaultReturnType, (object result, Action cleanup)>> _returnTypeMap = new()
        {
            //TODO: who frees the string? make sure you handle leakage
            {typeof(string), managedResult =>
                {
                    
                    var asManagedString = Assersions.NoException(()=>Marshal.PtrToStringBSTR(managedResult), "Error marshaling BSTR result");
                    void Free() => PInvoke.SysFreeString(managedResult); //TODO: look at return value
                    return (asManagedString, Free);
                }
            }
        };

        private readonly Dictionary<Type, Func<object, (object result, Action cleanup)>> _outputArgumentsTypepMap =
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
            return _outputArgumentsTypepMap.GetValueOrNone(argument.GetType()).Match(func => func(argument), () => (argument, () => { }));
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