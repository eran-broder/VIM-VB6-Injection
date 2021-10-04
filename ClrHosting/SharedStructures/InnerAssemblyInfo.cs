using System;
using System.Runtime.InteropServices;

namespace SharedStructures
{

    public interface IContextManager
    {
        public void UnloadCurrent();
    }

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
    public struct MainClrInfo
    {
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 256)]
        public string pathToClr;
        
        //TODO: pass an array. not this crap
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 512)]
        public string trustedDirectories;

        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 256)]
        public string assemblyName;
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 256)]
        public string nameOfClass;
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 256)]
        public string methodName;

        public AssemblyRunnerInfo AssemblyInfo;
    }

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
    public struct AssemblyRunnerInfo
    {
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 256)]
        public string assemblyPath;
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 256)]
        public string nameOfClass;
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 256)]
        public string methodName;

        public ArgumentsForManagedLibrary ArgumentsForManagedLibrary;
    }

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
    public struct ArgumentsForManagedLibrary
    {
        public IntPtr WindowToHook;
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 256)]
        public string PathOfInjectedDll;
    }

}
