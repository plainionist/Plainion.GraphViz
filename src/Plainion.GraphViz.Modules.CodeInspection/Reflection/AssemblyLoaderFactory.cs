namespace Plainion.GraphViz.Modules.CodeInspection.Reflection
{
    enum DotNetRuntime
    {
        Core,
        Framework
    }

    class AssemblyLoaderFactory
    {
        public static IAssemblyLoader Create(DotNetRuntime runtime) => new ReflectionOnlyAssemblyLoader(runtime);
    }
}