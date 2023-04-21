using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;

namespace Plainion.GraphViz.Modules.CodeInspection.Common.Analyzers
{
    internal class MscorlibResolver : AbstractAssemblyResolver<AssemblyResolutionResult>
    {
        public MscorlibResolver()
            : base(VersionMatchingStrategy.Exact)
        {
        }

        public override IReadOnlyCollection<AssemblyResolutionResult> TryResolve(AssemblyName assemblyName, Assembly requestingAssembly = null)
        {
            if (assemblyName.Name == "mscorlib" && assemblyName.Version == new Version(4, 0, 0, 0))
            {
                var netFwRoot = assemblyName.ProcessorArchitecture == ProcessorArchitecture.Amd64
                    ? @"%systemroot%\Microsoft.NET\Framework64"
                    : @"%systemroot%\Microsoft.NET\Framework";

                netFwRoot = Environment.ExpandEnvironmentVariables(netFwRoot);

                var version = Directory.GetDirectories(netFwRoot, "v4.0.*")
                    .Select(Path.GetFileName)
                    .OrderBy(x => x)
                    .Last();

                return new[] { new AssemblyResolutionResult(new FileInfo(Path.Combine(netFwRoot, version, "mscorlib.dll"))) };
            }

            return Array.Empty<AssemblyResolutionResult>();
        }
    }
}
