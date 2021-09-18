using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace SetHookForInjection
{
    public static class ThreadInjector
    {
        
        [DllImport(@"C:\vim\ParameterizedInjector\Release\ParameterizedInjectorLib.dll")]
        public static extern int Inject(int processId, //TODO: should be uint
                                        string dllName, 
                                        string functionName, 
                                        IntPtr userData, 
                                        int dataSize,
                                        out uint retCode);
    }
}
