using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Plainion.GraphViz.Modules.CodeInspection.Reflection
{
    internal class MscorlibResolutionResult : AssemblyResolutionResult
    {
        public MscorlibResolutionResult(FileInfo file, params string[] referenceAssemblies)
            : base(file)
        {
            ReferenceAssemblies = referenceAssemblies
                .Select(x => new DirectoryInfo(x))
                .ToList();
        }

        public IReadOnlyCollection<DirectoryInfo> ReferenceAssemblies { get; }
    }
}