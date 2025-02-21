namespace Plainion.GraphViz.Modules.CodeInspection.Reflection
{
    public enum DotNetRuntime
    {
        Core,
        Framework
    }

    public class AssemblyLoaderFactory
    {
        public static IAssemblyLoader Create(DotNetRuntime runtime) => new ReflectionOnlyAssemblyLoader(runtime);
    }
}