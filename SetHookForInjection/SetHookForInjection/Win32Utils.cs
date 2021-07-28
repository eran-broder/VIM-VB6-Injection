using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SetHookForInjection
{
    public static class Win32Utils
    {

        public static IEnumerable<IntPtr> GetTopLevelWindowsOfProcess(int pid)
        {
            var result = new List<IntPtr>();
            bool EnumWindowsProc(IntPtr hWnd, IntPtr lParam)
            {
                var threadId = PInvoke.GetWindowThreadProcessId(hWnd, out var currentWindowProcessId);
                if (pid == currentWindowProcessId)
                {
                    result.Add(hWnd);
                }
                return true;
            }

            PInvoke.EnumChildWindows(IntPtr.Zero, EnumWindowsProc, IntPtr.Zero);

            return result;
        }

        public static string GetText(IntPtr hWnd)
        {
            // Allocate correct string length first
            int length = PInvoke.GetWindowTextLength(hWnd);
            StringBuilder sb = new StringBuilder(length + 1);
            PInvoke.GetWindowText(hWnd, sb, sb.Capacity);
            return sb.ToString();
        }

    }
}
