using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

//TODO: all these methods are a mirror of our dll. perhaps auto-generate it?
namespace ManagedLibraryForInjection
{
    public class EcwEmbeddedAdapterProxy
    {
        [DllImport("called.dll", SetLastError = true)]
        public static extern void SetReferral();
    }
}
