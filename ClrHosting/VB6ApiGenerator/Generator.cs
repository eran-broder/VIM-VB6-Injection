using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace VB6ApiGenerator
{
    class CodeTextBuilder
    {
        private readonly StringBuilder _sb = new StringBuilder();
        private int _tabCount = 0;
        private string _tabLiteral = "    ";

        public void w(string text)
        {
            _sb.Append(text);
        }

        public void wl(string text){
            w(text);
            w("\n");
        }

        private void DoIndent() => w(string.Concat(Enumerable.Repeat(_tabLiteral, _tabCount)));

        public void Indent() => _tabCount++;
        public void DeIndent() => _tabCount--;

        public string Text() => _sb.ToString();

        public void Block(Action textAction)
        {
            wl($"{{");
            Indent();
            textAction();
            DeIndent();
            wl($"}}");

        }
    }

    public class Names
    {
        public const string ClassName = "EcwProxy";
    }

    public class Generator
    {
        private CodeTextBuilder b = new CodeTextBuilder();
        public static string Generate(IEnumerable<MethodInfo> methods)
        {
            return (new Generator()).DoGenerate(methods);
        }
        
        private string DoGenerate(IEnumerable<MethodInfo> methods)
        {
            Imports();
            Using("System");
            Using("VB6ApiGenerator");
            Namespace("EcwProxy");
            b.Block(() =>
            {
                Class(Names.ClassName, "NativeDllWrapperBase");
                b.Block(()=>ClassContent(methods));
            });
            
            return b.Text();
        }

        private void Using(string @namespace)
        {
            b.wl($"using {@namespace};");
        }

        private void ClassContent(IEnumerable<MethodInfo> methodInfos)
        {
            Constructor("called.dll");
            methodInfos.ToList().ForEach(GenerateMethodWrapper);
        }

        private void Constructor(string calledDll)
        {
            b.wl($"public {Names.ClassName}(string dllName):base(dllName){{}}");
        }

        private void GenerateMethodWrapper(MethodInfo method)
        {
            var delegateName = $"{method.Name}_delegate";
            b.wl($"private delegate {method.ReturnType.Name} {delegateName} ({Join(GetParametersWithTypes(method))});");
            b.w($"public {GetMethodDeclaration(method)}");
            b.Block(() =>
            {
                b.wl($"var methodDelegate = GetDelegateByName(\"{method.Name}\", typeof({delegateName}))");
                b.wl($"methodDelegate({Join(GetParametersNames(method))})");
            });
        }


        private string Join(IEnumerable<string> parts) => string.Join(", ", parts);

        private IEnumerable<string> GetParametersNames(MethodInfo method) =>
            method.GetParameters().Select(p => p.Name);

        private IEnumerable<string> GetParametersWithTypes(MethodInfo method) =>
            method.GetParameters().Select(p => $"{p.ParameterType.Name} {p.Name}");

        private string GetMethodDeclaration(MethodInfo method)=>
            $"{method.ReturnType.Name} {method.Name} ({Join(GetParametersWithTypes(method))})";
        

        private void Class(string ecwproxy, string baseClass=null)
        {
            var baseClassSuffix = string.IsNullOrEmpty(baseClass) ? "" : $": {baseClass}";
            b.w($"public class {ecwproxy}{baseClassSuffix}");
        }

        private void Namespace(string name)
        {
            b.w($"namespace {name}");
        }

        private void Imports()
        {
            var namespaces = new[] { "System" };
        }

        private void WriteImport(string @namespace)
        {
            b.wl($"using {@namespace};");
        }
    }
}
