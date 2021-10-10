using System;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text.Json;
using CommandLine;
using SharedStructures;

namespace Injector
{
    class Program
    {

        //TODO: should I bundle the dll with the exe? probably. but how?
        [DllImport(@"C:\vim\ParameterizedInjector\Release\ParameterizedInjectorLib.dll")]
        public static extern int Inject(uint processId,
            string dllName,
            string functionName,
            IntPtr userData,
            int dataSize,
            out uint retCode);


        private class Options
        {
            [Option('p', "pid", Required = true, HelpText = "process Id")]
            public int ProcessId { get; set; }

            [Option('d', "dll", Required = true, HelpText = "path to the json describing the options")]
            public string DllName { get; set; }

            [Option('f', "function", Required = true, HelpText = "path to the json describing the options")]
            public string FunctionName { get; set; }

            [Option('d', "b64data", Required = true, HelpText = "base64 data payload")]
            public string UserData { get; set; }
        }

        static void Main(string[] args)
        {
            try
            {
                Parser.Default.ParseArguments<Options>(args)
                    .WithParsed(options => Run(options));
            }
            catch(Exception e)
            {
                Console.WriteLine(e);
            }
        }

        private static unsafe void Run(Options opts)
        {
            var data = Convert.FromBase64String(opts.UserData);
            fixed (byte* pData = data)
            {
                Console.WriteLine("About to inject");
                var result = Inject((uint)opts.ProcessId, opts.DllName, opts.FunctionName, (IntPtr) pData, data.Length, out var ret);
                Console.WriteLine($"Injection direct result = [{result}]. function result = [{ret}]");
            }
        }
    }
}
