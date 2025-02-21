using System;
using System.Reflection;

namespace Plainion.GraphViz.Modules.CodeInspection.Common
{
    class R
    {
        public static string AssemblyName(Assembly asm)
        {
            return asm.GetName().Name;
        }

        public static string TypeFullName(Type t)
        {
            return t.FullName ?? $"{t.Namespace}.{t.Name}";
        }
    }
}
