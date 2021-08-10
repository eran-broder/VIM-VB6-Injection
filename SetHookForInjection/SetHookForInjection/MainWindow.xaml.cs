using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Path = System.IO.Path;

namespace SetHookForInjection
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            Loaded += (sender, args) => Go();
        }

        private (string pathOfExe, string pathOfInjectedDll, string pathOfWorkerDll) GetFilesForDemo()
        {
            var baseFolder = System.IO.Path.GetFullPath(@"../../../../../");
            string Full(string relative) => Path.Join(baseFolder, relative);
            return (Full(@"Playground App\Caller.exe"),
                    Full(@"Injected Dll\Called.dll"),
                    Full(@"SetHookForInjection\Debug\DllThatLoadsClr.dll")
                    );
        }

        private void Go()
        {
            var (pathOfExe, pathOfInjectedDll, pathOfWorkerDll) = GetFilesForDemo();
            CopyWorkerDllToWorkingDirectory(pathOfExe, pathOfWorkerDll);
            var (handleOfWindow, _) = GetWindowHandleToInject(pathOfExe, "Form1");
            var threadId = PInvoke.GetWindowThreadProcessId(handleOfWindow, out var processId);

            UpdateUi(processId, threadId, handleOfWindow);
            var dll = PInvoke.LoadLibrary(pathOfInjectedDll);

            if (dll == IntPtr.Zero)
            {
                throw new Exception($"module could not be loaded [{Marshal.GetLastWin32Error()}]");
            }

            var addressAsIntPtr = PInvoke.GetProcAddress(dll, "KeyboardProc");
            var addressAsDelegate = Marshal.GetDelegateForFunctionPointer<PInvoke.HookProc>(addressAsIntPtr);
            

            if (addressAsIntPtr == IntPtr.Zero)
            {
                throw new Exception($"Cannot find function");
            }
            
            //TODO: change hook type
            var hoolHandle = PInvoke.SetWindowsHookEx(PInvoke.HookType.WH_GETMESSAGE, addressAsDelegate, dll, threadId);

            ////TODO: trigger the hook and validate return value as a handshake
            Thread.Sleep(TimeSpan.FromSeconds(2));
            PInvoke.PostMessage(handleOfWindow, 1029, 222, new IntPtr(333));

            
            //PInvoke.UnhookWindowsHookEx(hoolHandle);

        }

        private void CopyWorkerDllToWorkingDirectory(string pathOfExe, string pathOfWorkerDll)
        {
            //var destPath = Path.Join(Path.GetDirectoryName(pathOfExe), Path.GetFileName(pathOfWorkerDll));
            var destPath = Path.Join(Path.GetDirectoryName(pathOfExe), "VimInjectedIpc.dll");
            File.Copy(pathOfWorkerDll, destPath, true);
        }

        private void UpdateUi(uint lpdwProcessId, uint threadId, IntPtr handleOfWindow)
        {
            TextBoxProcessId.Text = lpdwProcessId.ToString();
            TextBoxThreadId.Text = threadId.ToString();
            TextBoxHandle.Text = handleOfWindow.ToInt32().ToString();
        }

        //DEMO
        private (IntPtr handle, int processId) GetWindowHandleToInject(string pathOfExe, string captionOfWindow)
        {
            var nameOfProcess = System.IO.Path.GetFileNameWithoutExtension(pathOfExe);
            var processes = Process.GetProcessesByName(nameOfProcess);
            processes.ToList().ForEach(p => p.Kill(true));
            var process = Process.Start(pathOfExe);
            Thread.Sleep(TimeSpan.FromMilliseconds(500));
            var handles = Win32Utils.GetTopLevelWindowsOfProcess(process.Id).ToList();
            var classes = handles.Select(Win32Utils.GetClassName).ToList();
            Console.WriteLine(classes);
            Console.WriteLine(handles);
            //var formHandle = handles.First(h => Win32Utils.GetText(h).Equals("Referral (Outgoing)"));
            var formHandle = handles.First(h => Win32Utils.GetClassName(h).Equals("ThunderRT6Main", StringComparison.InvariantCultureIgnoreCase));
            return (formHandle, process.Id);
        }

    }
}
