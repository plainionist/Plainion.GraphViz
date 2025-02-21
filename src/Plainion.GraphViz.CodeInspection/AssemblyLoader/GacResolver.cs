using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;

namespace Plainion.GraphViz.CodeInspection.AssemblyLoader;

class GacResolver : AbstractAssemblyResolver<AssemblyResolutionResult>
{
    private const string myGacLocation = @"C:\Windows\Microsoft.NET\assembly\GAC_MSIL";

    public GacResolver()
        : base(VersionMatchingStrategy.Exact)
    {
    }

    // C:\Windows\Microsoft.NET\assembly\GAC_MSIL\PresentationUI\v4.0_4.0.0.0__31bf3856ad364e35
    public override IReadOnlyCollection<AssemblyResolutionResult> TryResolve(AssemblyName assemblyName, Assembly requestingAssembly)
    {
        var assemblyDir = Path.Combine(myGacLocation, assemblyName.Name);
        if (!Directory.Exists(assemblyDir))
        {
            return Array.Empty<AssemblyResolutionResult>();
        }

        var versionDir = Directory.GetDirectories(assemblyDir)
            .OrderByDescending(x => x)
            .FirstOrDefault();
        if (versionDir == null)
        {
            return Array.Empty<AssemblyResolutionResult>();
        }

        var assemblyFile = new FileInfo(Path.Combine(versionDir, assemblyName.Name + ".dll"));
        return assemblyFile.Exists
            ? new[] { new AssemblyResolutionResult(assemblyFile) }
            : Array.Empty<AssemblyResolutionResult>();
    }
}
