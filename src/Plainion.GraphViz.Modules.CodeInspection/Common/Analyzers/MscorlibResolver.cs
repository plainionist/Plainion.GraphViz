using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;

namespace Plainion.GraphViz.Modules.CodeInspection.Common.Analyzers
{
    internal class MscorlibResolver : AbstractAssemblyResolver<MscorlibResolutionResult>
    {
        public MscorlibResolver()
            : base(VersionMatchingStrategy.Exact)
        {
        }

        public override IReadOnlyCollection<MscorlibResolutionResult> TryResolve(AssemblyName assemblyName, Assembly requestingAssembly)
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

                var result = new MscorlibResolutionResult(
                    new FileInfo(Path.Combine(netFwRoot, version, "mscorlib.dll")),
                    @"C:\Program Files (x86)\Reference Assemblies\Microsoft\Framework\.NETFramework\v4.8");

                return new[] { result };
            }

            return Array.Empty<MscorlibResolutionResult>();
        }
    }
}
