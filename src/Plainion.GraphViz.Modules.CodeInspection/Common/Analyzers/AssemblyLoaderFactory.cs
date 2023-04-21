namespace Plainion.GraphViz.Modules.CodeInspection.Common.Analyzers
{
    class AssemblyLoaderFactory
    {
        public static IAssemblyLoader Create() => new ReflectionOnlyAssemblyLoader();
    }
}