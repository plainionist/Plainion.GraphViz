using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;

namespace Plainion.GraphViz.Modules.CodeInspection.Common.Analyzers
{
    public class RelativePathResolver : AbstractAssemblyResolver<AssemblyResolutionResult>
    {
        private readonly SearchOption mySearchOption;

        public RelativePathResolver(VersionMatchingStrategy assemblyMatchingStrategy, SearchOption searchOption)
            : base(assemblyMatchingStrategy)
        {
            mySearchOption = searchOption;
        }

        public override IReadOnlyCollection<AssemblyResolutionResult> TryResolve(AssemblyName assemblyName, Assembly requestingAssembly)
        {
            Contract.RequiresNotNull(assemblyName, nameof(assemblyName));
            Contract.RequiresNotNull(requestingAssembly, nameof(requestingAssembly));

            var searchDir = new FileInfo(requestingAssembly.Location).Directory;

            return searchDir.EnumerateFiles($"{assemblyName.Name}.dll", mySearchOption)
                .Select(x => new AssemblyResolutionResult(x))
                .Where(x => assemblyName.Name == x.AssemblyName.Name
                    && assemblyName.Version.Matches(x.AssemblyName.Version, myAssemblyMatchingStrategy)
                    && IsSupportedArchitecture(x.AssemblyName))
                .ToList();
        }
    }
}
