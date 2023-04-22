using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;

namespace Plainion.GraphViz.Modules.CodeInspection.Reflection
{
    internal class MscorlibResolver : AbstractAssemblyResolver<MscorlibResolutionResult>
    {
        private readonly DotNetRuntime myDotnetRuntime;

        public MscorlibResolver(DotNetRuntime dotnetRuntime)
            : base(VersionMatchingStrategy.Exact)
        {
            myDotnetRuntime = dotnetRuntime;
        }

        public override IReadOnlyCollection<MscorlibResolutionResult> TryResolve(AssemblyName assemblyName, Assembly requestingAssembly)
        {
            if (assemblyName.Name != "mscorlib")
            {
                return Array.Empty<MscorlibResolutionResult>();
            }

            if (myDotnetRuntime == DotNetRuntime.Framework)
            {
                var netFwRoot = assemblyName.ProcessorArchitecture == ProcessorArchitecture.Amd64
                    ? @"%systemroot%\Microsoft.NET\Framework64"
                    : @"%systemroot%\Microsoft.NET\Framework";

                netFwRoot = Environment.ExpandEnvironmentVariables(netFwRoot);

                var version = Directory.GetDirectories(netFwRoot, "v4.0.*")
                    .Select(Path.GetFileName)
                    .OrderByDescending(x => x)
                    .First();

                var result = new MscorlibResolutionResult(
                    new FileInfo(Path.Combine(netFwRoot, version, "mscorlib.dll")),
                    @"C:\Program Files (x86)\Reference Assemblies\Microsoft\Framework\.NETFramework\v4.8");

                return new[] { result };
            }
            else
            {
                var netRoot = @"C:\Program Files\dotnet\shared\Microsoft.NETCore.App";

                var version = Directory.GetDirectories(netRoot)
                    .Select(Path.GetFileName)
                    .OrderByDescending(x => x)
                    .First();

                var result = new MscorlibResolutionResult(
                    new FileInfo(Path.Combine(netRoot, version, "mscorlib.dll")),
                    Path.Combine(@"C:\Program Files\dotnet\shared\Microsoft.AspNetCore.App", version),
                    Path.Combine(@"C:\Program Files\dotnet\shared\Microsoft.WindowsDesktop.App", version));

                return new[] { result };
            }
        }
    }
}
