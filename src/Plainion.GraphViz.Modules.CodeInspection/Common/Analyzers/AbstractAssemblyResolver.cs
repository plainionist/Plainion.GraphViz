using System;
using System.Collections.Generic;
using System.Reflection;

namespace Plainion.GraphViz.Modules.CodeInspection.Common.Analyzers
{
    public abstract class AbstractAssemblyResolver<T> where T : AssemblyResolutionResult
    {
        protected readonly VersionMatchingStrategy myAssemblyMatchingStrategy;

        public AbstractAssemblyResolver(VersionMatchingStrategy assemblyMatchingStrategy)
        {
            myAssemblyMatchingStrategy = assemblyMatchingStrategy;
        }

        public abstract IReadOnlyCollection<T> TryResolve(AssemblyName assemblyName, Assembly requestingAssembly);

        protected static bool IsSupportedArchitecture(AssemblyName assemblyName) =>
            assemblyName.ProcessorArchitecture == ProcessorArchitecture.MSIL ||
                assemblyName.ProcessorArchitecture == (Environment.Is64BitProcess ? ProcessorArchitecture.Amd64 : ProcessorArchitecture.X86);
    }
}
