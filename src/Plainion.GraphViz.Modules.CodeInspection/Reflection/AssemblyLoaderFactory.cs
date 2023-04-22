namespace Plainion.GraphViz.Modules.CodeInspection.Reflection
{
    class AssemblyLoaderFactory
    {
        public static IAssemblyLoader Create() => new ReflectionOnlyAssemblyLoader();
    }
}