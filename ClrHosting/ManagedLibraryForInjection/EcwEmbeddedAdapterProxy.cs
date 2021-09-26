using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

//TODO: all these methods are a mirror of our dll. perhaps auto-generate it?
namespace ManagedLibraryForInjection
{
    //TODO: make this auto generated or abolish it
    public class EcwEmbeddedAdapterProxy
    {
        [DllImport("called.dll", SetLastError = true, CallingConvention = CallingConvention.StdCall)]
        public static extern int SetReferral();

        [DllImport("called.dll", SetLastError = true, CallingConvention = CallingConvention.StdCall)]
        public static extern int TheRealShitAddAssessment();
    }
}
