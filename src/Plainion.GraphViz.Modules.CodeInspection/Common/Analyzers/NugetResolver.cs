﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using Plainion;

namespace Plainion.GraphViz.Modules.CodeInspection.Common.Analyzers
{
    public class NugetResolver : AbstractAssemblyResolver<NugetAssemblyResolutionResult>
    {
        private readonly IReadOnlyCollection<DirectoryInfo> myNugetCaches;
        private readonly VersionMatchingStrategy myPackageMatchingStrategy;

        public NugetResolver(VersionMatchingStrategy assemblyMatchingStrategy, VersionMatchingStrategy packageMatchingStrategy, IEnumerable<DirectoryInfo> caches = null)
            : base(assemblyMatchingStrategy)
        {
            myPackageMatchingStrategy = packageMatchingStrategy;
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

        public override IReadOnlyCollection<NugetAssemblyResolutionResult> TryResolve(AssemblyName assemblyName, Assembly requestingAssembly = null)
        {
            Contract.RequiresNotNull(assemblyName, nameof(assemblyName));

            var frameworkVersion = DotNetFrameworkVersion.TryParse(requestingAssembly ?? Assembly.GetEntryAssembly());
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
                .SingleOrDefault(x => x.Name == assemblyName.Name);

            if (packageDir == null)
            {
                return new List<NugetAssemblyResolutionResult>();
            }

            return packageDir.EnumerateFiles($"{assemblyName.Name}.dll", SearchOption.AllDirectories)
                .Select(x => NugetAssemblyResolutionResult.TryCreate(x))
                .Where(x => x != null)
                .Where(x => x.AssemblyName.Name == assemblyName.Name)
                .Where(x => assemblyName.Version.Matches(x.AssemblyName.Version, myAssemblyMatchingStrategy))
                .Where(x => assemblyName.Version.Matches(new Version(x.PackageVersion.Major, x.PackageVersion.Minor, x.PackageVersion.Patch), myPackageMatchingStrategy))
                .Where(x => IsSupportedArchitecture(x.AssemblyName));
        }
    }
}