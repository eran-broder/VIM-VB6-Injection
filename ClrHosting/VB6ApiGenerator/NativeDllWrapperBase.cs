using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using Brotils;
using Optional.Collections;
using Win32Utils;

namespace VB6ApiGenerator
{
    public abstract class NativeDllWrapperBase
    {
        private readonly Func<string, Type, Delegate> _getter;


        private static Func<string, Type, Delegate> CreateGetter(string dllName)
        {
            var hmod = PInvoke.LoadLibrary(dllName);
            Assersions.Assert(hmod != IntPtr.Zero, $"Failed loading library from [{dllName}]");

            Delegate Getter(string functionName, Type type)
            {
                var proc = PInvoke.GetProcAddress(hmod, functionName);
                var @delegate = Marshal.GetDelegateForFunctionPointer(proc, type);
                return @delegate;
            }

            return Getter;
        }

        protected NativeDllWrapperBase(string dllName)
        {
            _getter = CreateGetter(dllName);
        }

        //TODO: probably whould be nice to cache
        protected TDelegate GetDelegateByName<TDelegate>(string name)
        {
            return (TDelegate)(object)_getter(name, typeof(TDelegate));
        }
    }
}
