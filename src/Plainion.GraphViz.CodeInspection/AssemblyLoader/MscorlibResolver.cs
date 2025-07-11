using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;

namespace Plainion.GraphViz.CodeInspection.AssemblyLoader;

/// <summary>
/// Resolves the entry point to the .NET runtime/SDK.
/// </summary>
class MscorlibResolver : AbstractAssemblyResolver<MscorlibResolutionResult>
{
    private readonly DotNetRuntime myDotnetRuntime;

    public MscorlibResolver(DotNetRuntime dotnetRuntime)
        : base(VersionMatchingStrategy.Exact)
    {
        myDotnetRuntime = dotnetRuntime;
    }

    public override IReadOnlyCollection<MscorlibResolutionResult> TryResolve(AssemblyName assemblyName, Assembly requestingAssembly)
    {
        if (assemblyName.Name != "mscorlib" && assemblyName.Name != "System.Runtime")
        {
            return Array.Empty<MscorlibResolutionResult>();
        }

        if (myDotnetRuntime == DotNetRuntime.Framework)
        {
            // TODO: no longer supported with .Net 8 -> lets check whether 32bit support is even needed
            //var netFwRoot = assemblyName.ProcessorArchitecture == ProcessorArchitecture.Amd64
            //    ? @"%systemroot%\Microsoft.NET\Framework64"
            //    : @"%systemroot%\Microsoft.NET\Framework";
            var netFwRoot = @"%systemroot%\Microsoft.NET\Framework64";

            netFwRoot = Environment.ExpandEnvironmentVariables(netFwRoot);

            var version = Directory.GetDirectories(netFwRoot, "v4.0.*")
                .Select(Path.GetFileName)
                .OrderByDescending(x => x)
                .First();

            var result = new MscorlibResolutionResult(
                new FileInfo(Path.Combine(netFwRoot, version, assemblyName.Name + ".dll")),
                @"C:\Program Files (x86)\Reference Assemblies\Microsoft\Framework\.NETFramework\v4.8");

            return [result];
        }
        else
        {
            var netRoot = @"C:\Program Files\dotnet\shared\Microsoft.NETCore.App";

            var version = Directory.GetDirectories(netRoot)
                .Select(Path.GetFileName)
                .OrderByDescending(x => x)
                .First();

            var result = new MscorlibResolutionResult(
                new FileInfo(Path.Combine(netRoot, version, assemblyName.Name + ".dll")),
                Path.Combine(@"C:\Program Files\dotnet\shared\Microsoft.AspNetCore.App", version),
                Path.Combine(@"C:\Program Files\dotnet\shared\Microsoft.WindowsDesktop.App", version));

            return [result];
        }
    }
}
