using System;
using System.Reflection;

namespace Plainion.GraphViz.Modules.CodeInspection.Batch
{
    public static class R
    {
        private static AssemblyLoader myLoader = new AssemblyLoader();

        public static string AssemblyName(Assembly asm)
        {
            return asm.GetName().Name;
        }

        public static string TypeFullName(Type t)
        {
            return t.FullName != null ? t.FullName : t.Namespace + t.Name;
        }

        public static Assembly LoadAssembly(AssemblyName name)
        {
            return myLoader.LoadAssembly(name);
        }

        public static Assembly LoadAssemblyFrom(string file)
        {
            return myLoader.LoadAssemblyFrom(file);
        }
    }
}
