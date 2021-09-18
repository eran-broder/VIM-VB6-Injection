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
using Win32Utils;
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

        private static string Full(string relative)
        {
            var baseFolder = Path.GetFullPath(@"../../../../../");
            return Path.Join(baseFolder, relative);
        }

        private static Dictionary<string, (string path, bool shouldKill)> configs =
            new()
            {
                {"test", (Full(@"Playground App\Caller.exe"), true)},
                { "ecw", (@"C:\Program Files (x86)\eClinicalWorks_MGSFL\eClinicalWorks.exe", false) }
            };

        private static string SelectedConfigName = "test";
        private IntPtr _hookHandle;
        private IntPtr _handleOfWindow;
        private Action _injectionAction;
        private IntPtr _calledHandle;
        private static (string path, bool shouldKill) SelectedConfig => configs[SelectedConfigName];
        private (string pathOfExe, string pathOfInjectedDll, IEnumerable<string> filesToCopy) GetFilesForDemo()
        {
            
            return (SelectedConfig.path,
                    Full(@"Injected Dll\Called.dll"),
                    new []
                    {
                        Full(@"ClrHosting\Debug\VimInProcessOrchestrator.dll") ,
                        Full(@"ClrHosting\Debug\DynamicClrHosting.dll") ,
                    }
                    //Full(@"SetHookForInjection\Release\DllThatLoadsClr.dll")
                    );
        }
        
        private void Go()
        {
            var (pathOfExe, pathOfInjectedDll, filesToCopy) = GetFilesForDemo();
            CopyWorkerDllToWorkingDirectory(pathOfExe, filesToCopy);

            var (handleOfWindow, _) = GetWindowHandleToInject(pathOfExe, SelectedConfig.shouldKill);
            _handleOfWindow = handleOfWindow;
            var threadId = PInvoke.GetWindowThreadProcessId(handleOfWindow, out var processId);

            UpdateUi(processId, threadId, handleOfWindow);

            _injectionAction = ()=>DoInject(pathOfInjectedDll, processId, threadId, handleOfWindow);

            Thread.Sleep(TimeSpan.FromSeconds(1));//TODO: very very bad. do not avoid understanding why it matters.

            _injectionAction();
            //TriggerClrLoading(handleOfWindow);
        }

        //new_injector::InjectionResult Inject(int process_id, LPCSTR dll_name, LPCSTR function_name, void* userData, size_t dataSize, DWORD* retCode)
        private void DoInject(string pathOfInjectedDll, uint processId, uint threadId, IntPtr handleOfWindow)
        {

            var files = GetFilesForDemo();
            var dllToInject = Full(@"ClrHosting\Debug\VimInProcessOrchestrator.dll");
            uint ret;

            var d = new DummyStruct() {value = handleOfWindow, 
                                       pathToClr = GetClrPath(),
                                       assemblyName = "ManagedAssemblyRunner, Version=1.0.0.0",
                                       nameOfClass = "ManagedAssemblyRunner.Runner",
                                       methodName = "DoWork"
            };
            var sizeOfPayload = Marshal.SizeOf(d);
            IntPtr pnt = Marshal.AllocHGlobal(sizeOfPayload);
            Marshal.StructureToPtr(d, pnt, false);
            var result = ThreadInjector.Inject((int)processId, 
                                                    dllToInject,
                                                    "VimStart2",
                                                    pnt, 
                                                    sizeOfPayload, 
                                                    out ret);
            MessageBox.Show($"Result [{result}]\nReturn [{ret}]");
        }

        private string GetClrPath()=>
            Full(@"ClrHosting\ManagedLibraryForInjection\bin\Debug\net5.0\coreclr.dll");
        

        //TODO: can it be made a record?
        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
        struct DummyStruct
        {
            public IntPtr value;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 256)] 
            public string pathToClr;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 256)]
            public string assemblyName;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 256)]
            public string nameOfClass;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 256)]
            public string methodName;
        }

        private void DoInjectOld(string pathOfInjectedDll, uint threadId, IntPtr handleOfWindow)
        {
            
            var dll = PInvoke.LoadLibrary(pathOfInjectedDll);
            _calledHandle = dll;

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
            _hookHandle = PInvoke.SetWindowsHookEx(PInvoke.HookType.WH_GETMESSAGE, addressAsDelegate, dll, threadId);
            if (_hookHandle == IntPtr.Zero)
            {
                MessageBox.Show("Error hooking window!");
            }
            

            //PInvoke.UnhookWindowsHookEx(hoolHandle);
        }

        private static void TriggerClrLoading(IntPtr handleOfWindow)
        {
            Task.Run(() =>
            {
                Thread.Sleep(TimeSpan.FromSeconds(1));
                PInvoke.PostMessage(handleOfWindow, 1029, 222, new IntPtr(333));
            });
        }

        private void CopyWorkerDllToWorkingDirectory(string pathOfExe, IEnumerable<string> filesToCopy)
        {
            
            string DestPath(string f) => Path.Join(Path.GetDirectoryName(pathOfExe), Path.GetFileName(f));
            filesToCopy.ToList().ForEach(f => File.Copy(f, DestPath(f), true));
        }

        private void UpdateUi(uint lpdwProcessId, uint threadId, IntPtr handleOfWindow)
        {
            TextBoxProcessId.Text = lpdwProcessId.ToString();
            TextBoxThreadId.Text = threadId.ToString();
            TextBoxHandle.Text = handleOfWindow.ToInt32().ToString();
        }

        //DEMO
        private (IntPtr handle, int processId) GetWindowHandleToInject(string pathOfExe, bool shouldKill)
        {
            var process = shouldKill ? RestartProcess(pathOfExe) : GetRunningProcess(pathOfExe);
            Thread.Sleep(TimeSpan.FromMilliseconds(500));
            var handles = Win32Utils.Win32Utils.GetTopLevelWindowsOfProcess(process.Id).ToList();
            var classes = handles.Select(Win32Utils.Win32Utils.GetClassName).ToList();
            Console.WriteLine(classes);
            Console.WriteLine(handles);
            //var formHandle = handles.First(h => Win32Utils.GetText(h).Equals("Referral (Outgoing)"));
            var formHandle = handles.First(h => Win32Utils.Win32Utils.GetClassName(h).Equals("ThunderRT6Main", StringComparison.InvariantCultureIgnoreCase));
            return (formHandle, process.Id);
        }

        private Process GetRunningProcess(string pathOfExe)
        {
            var nameOfProcess = System.IO.Path.GetFileNameWithoutExtension(pathOfExe);
            var processes = Process.GetProcessesByName(nameOfProcess);
            return processes.First();
        }
        private Process RestartProcess(string pathOfExe)
        {
            var nameOfProcess = System.IO.Path.GetFileNameWithoutExtension(pathOfExe);
            var processes = Process.GetProcessesByName(nameOfProcess);
            processes.ToList().ForEach(p => p.Kill(true));
            var process = Process.Start(pathOfExe);
            process.WaitForInputIdle();
            return process;
        }

        private void ButtonUnloadClr_Click(object sender, RoutedEventArgs e)
        {
            PInvoke.PostMessage(_handleOfWindow, 1032, 0, IntPtr.Zero);
        }

        private void ButtonUnloadHook_Click(object sender, RoutedEventArgs e)
        {
            PInvoke.UnhookWindowsHookEx(_hookHandle);
            PInvoke.FreeLibrary(_calledHandle);
        }

        private void ButtonReloadAll_Click(object sender, RoutedEventArgs e)
        {
            _injectionAction();
        }

        private void ButtonSendLoadMessage_OnClick(object sender, RoutedEventArgs e)
        {
            TriggerClrLoading(_handleOfWindow);
        }
    }
}
