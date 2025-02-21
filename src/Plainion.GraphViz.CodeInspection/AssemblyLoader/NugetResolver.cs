using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using Plainion;

namespace Plainion.GraphViz.CodeInspection.AssemblyLoader;

class NugetResolver : AbstractAssemblyResolver<NugetAssemblyResolutionResult>
{
    private readonly IReadOnlyCollection<DirectoryInfo> myNugetCaches;

    public NugetResolver(VersionMatchingStrategy assemblyMatchingStrategy, IEnumerable<DirectoryInfo> caches = null)
        : base(assemblyMatchingStrategy)
    {
        myNugetCaches = (caches ?? EnumerateCaches()).ToList();
    }

    private static IEnumerable<DirectoryInfo> EnumerateCaches()
    {
        var userProfileDir = new DirectoryInfo(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile));

        string nugetDir = Path.Combine(userProfileDir.FullName, ".nuget", "packages");
        if (Directory.Exists(nugetDir))
        {
            yield return new DirectoryInfo(nugetDir);
        }
    }

    public override IReadOnlyCollection<NugetAssemblyResolutionResult> TryResolve(AssemblyName assemblyName, Assembly requestingAssembly)
    {
        Contract.RequiresNotNull(assemblyName, nameof(assemblyName));
        Contract.RequiresNotNull(requestingAssembly, nameof(requestingAssembly));

        var frameworkVersion = DotNetFrameworkVersion.TryParse(requestingAssembly);
        if (frameworkVersion == null)
        {
            return new List<NugetAssemblyResolutionResult>();
        }

        var compatibleFrameworks = frameworkVersion.GetCompatibleFrameworkVersions();
        if (compatibleFrameworks.Count == 0)
        {
            return new List<NugetAssemblyResolutionResult>();
        }

        return myNugetCaches
            .SelectMany(x => EnumerateAssemblyCandidatesFromCache(assemblyName, x))
            .Where(x => compatibleFrameworks.Contains(x.DotNetFrameworkVersion))
            .OrderByDescending(d => d.PackageVersion)
            .ThenByDescending(d => d.DotNetFrameworkVersion)
            .ToList();
    }

    private IEnumerable<NugetAssemblyResolutionResult> EnumerateAssemblyCandidatesFromCache(AssemblyName assemblyName, DirectoryInfo cacheDir)
    {
        var packageDir = cacheDir.EnumerateDirectories(assemblyName.Name, SearchOption.TopDirectoryOnly)
            .SingleOrDefault(x => x.Name.Equals(assemblyName.Name, StringComparison.OrdinalIgnoreCase));

        if (packageDir == null)
        {
            return new List<NugetAssemblyResolutionResult>();
        }

        var results = packageDir.EnumerateFiles($"{assemblyName.Name}.dll", SearchOption.AllDirectories)
            .Select(x => NugetAssemblyResolutionResult.TryCreate(x))
            .Where(x => x != null)
            .ToList();

        return results
            .Where(x => x.AssemblyName.Name == assemblyName.Name)
            .Where(x => assemblyName.Version.Matches(x.AssemblyName.Version, myAssemblyMatchingStrategy))
            .Where(x => IsSupportedArchitecture(x.AssemblyName));
    }
}
