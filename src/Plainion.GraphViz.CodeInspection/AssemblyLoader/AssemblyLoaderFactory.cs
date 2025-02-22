using Microsoft.Extensions.Logging;

namespace Plainion.GraphViz.CodeInspection.AssemblyLoader;

public enum DotNetRuntime
{
    Core,
    Framework
}

public class AssemblyLoaderFactory
{
    public static IAssemblyLoader Create(ILoggerFactory loggerFactory, DotNetRuntime runtime) =>
        new ReflectionOnlyAssemblyLoader(loggerFactory, runtime);
}