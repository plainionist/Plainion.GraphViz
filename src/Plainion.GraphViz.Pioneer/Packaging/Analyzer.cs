
using System;
using System.Collections.Generic;
using System.Linq;
using Mono.Cecil;
using Mono.Cecil.Cil;

namespace Plainion.GraphViz.Pioneer.Packaging
{
    class Analyzer
    {
        private static AssemblyDefinition _assembly = AssemblyDefinition.ReadAssembly(
            System.Reflection.Assembly.GetExecutingAssembly().Location);

        private static void Method1()
        {
            Method2();
        }

        private static void Method2()
        {
            Method1();
            Method3();
        }

        private static void Method3()
        {
            Method1();
        }

        private static IEnumerable<MethodDefinition> GetMethodsCalled(
            MethodDefinition caller)
        {
            return caller.Body.Instructions
                .Where(x => x.OpCode == OpCodes.Call)
                .Select(x => (MethodDefinition)x.Operand);
        }

        private static MethodDefinition GetMethod(string name)
        {
            TypeDefinition programType = _assembly.MainModule.Types
                .FirstOrDefault(x => x.Name == "Analyzer");
            return programType.Methods.First(x => x.Name == name);
        }

        // http://stackoverflow.com/questions/24680054/how-to-get-the-list-of-methods-called-from-a-method-using-reflection-in-c-sharp
        internal void Execute(object rawConfig)
        {
            var config = (Config)rawConfig;

            MethodDefinition method1 = GetMethod("Method1");
            MethodDefinition method2 = GetMethod("Method2");
            MethodDefinition method3 = GetMethod("Method3");

            Console.WriteLine(CallsMethod(method1, method3) == false);
            Console.WriteLine(CallsMethod(method1, method2) == true);
            Console.WriteLine(CallsMethod(method3, method1) == true);

            Console.WriteLine(GetMethodsCalled(method2).SequenceEqual(
                new List<MethodDefinition> { method1, method3 }));
        }

        public static bool CallsMethod(MethodDefinition caller, MethodDefinition callee)
        {
            return caller.Body.Instructions.Any(x =>
                x.OpCode == OpCodes.Call && x.Operand == callee);
        }
    }
}
