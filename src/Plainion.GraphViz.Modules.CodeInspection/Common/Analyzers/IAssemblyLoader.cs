using System.Reflection;

namespace Plainion.GraphViz.Modules.CodeInspection.Common.Analyzers
{
    internal interface IAssemblyLoader
    {
        Assembly TryLoadAssembly(string path);
        Assembly TryLoadDependency(Assembly requestingAssembly, AssemblyName dependency);
    }

    class AssemblyLoaderFactory
    {
        public static IAssemblyLoader Create() => new FullAssemblyLoader();
    }
}