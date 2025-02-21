using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Plainion.GraphViz.CodeInspection.AssemblyLoader;

class MscorlibResolutionResult : AssemblyResolutionResult
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