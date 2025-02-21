namespace Plainion.GraphViz.CodeInspection.AssemblyLoader;

public enum DotNetRuntime
{
    Core,
    Framework
}

public class AssemblyLoaderFactory
{
    public static IAssemblyLoader Create(DotNetRuntime runtime) => new ReflectionOnlyAssemblyLoader(runtime);
}