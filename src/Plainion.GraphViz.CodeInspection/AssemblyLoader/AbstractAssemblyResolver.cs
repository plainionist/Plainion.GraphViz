using System.Collections.Generic;
using System.Reflection;

namespace Plainion.GraphViz.CodeInspection.AssemblyLoader;

abstract class AbstractAssemblyResolver<T> where T : AssemblyResolutionResult
{
    protected readonly VersionMatchingStrategy myAssemblyMatchingStrategy;

    public AbstractAssemblyResolver(VersionMatchingStrategy assemblyMatchingStrategy)
    {
        myAssemblyMatchingStrategy = assemblyMatchingStrategy;
    }

    public abstract IReadOnlyCollection<T> TryResolve(AssemblyName assemblyName, Assembly requestingAssembly);

    protected static bool IsSupportedArchitecture(AssemblyName assemblyName) => true;
    // TODO: no longer supported with .Net 8 -> lets check whether 32bit support is even needed
    //assemblyName.ProcessorArchitecture == ProcessorArchitecture.MSIL ||
    //    assemblyName.ProcessorArchitecture == (Environment.Is64BitProcess ? ProcessorArchitecture.Amd64 : ProcessorArchitecture.X86);
}
