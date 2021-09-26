using System;
using System.Runtime.InteropServices;

namespace SharedStructures
{
    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
    public struct MainClrInfo
    {
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 256)]
        public string pathToClr;
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
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 256)] //TODO: must I specify the length?
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
