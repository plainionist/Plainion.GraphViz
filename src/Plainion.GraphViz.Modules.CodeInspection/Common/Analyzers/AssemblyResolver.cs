using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using Nuclear.Assemblies;
using Nuclear.Assemblies.Factories;
using Nuclear.Assemblies.ResolverData;
using Nuclear.Assemblies.Resolvers;
using Nuclear.Creation;
using Plainion.Logging;

namespace Plainion.GraphViz.Modules.CodeInspection.Common.Analyzers
{
    class AssemblyResolver : IDisposable
    {
        private static readonly ILogger myLogger = LoggerFactory.GetLogger(typeof(AssemblyResolver));

        private readonly IDefaultResolver myDefaultResolver;
        private readonly INugetResolver myNuGetResolver;

        public AssemblyResolver()
        {
            Factory.Instance.DefaultResolver().Create(out myDefaultResolver, VersionMatchingStrategies.Strict, SearchOption.AllDirectories);
            Factory.Instance.NugetResolver().Create(out myNuGetResolver, VersionMatchingStrategies.Strict, VersionMatchingStrategies.Strict);

            AppDomain.CurrentDomain.AssemblyResolve += OnAssemblyResolve;
        }

        public void Dispose()
        {
            AppDomain.CurrentDomain.AssemblyResolve -= OnAssemblyResolve;
        }

        private Assembly OnAssemblyResolve(object sender, ResolveEventArgs args)
        {
            myLogger.Info($"Trying to resolve: {args.Name}");

            return TryResolve(myDefaultResolver, args) ?? TryResolve(myNuGetResolver, args);
        }

        private Assembly TryResolve<T>(IAssemblyResolver<T> resolver, ResolveEventArgs args) where T : IAssemblyResolverData =>
            resolver.TryResolve(args, out var result) ? TryLoad(result) : null;

        private Assembly TryLoad<T>(IEnumerable<T> result) where T : IAssemblyResolverData
        {
            foreach (var item in result)
            {
                if (AssemblyHelper.TryLoadFrom(item.File, out Assembly assembly))
                {
                    return assembly;
                }
            }
            return null;
        }

        public Assembly TryResolve(Assembly requestingAssembly, AssemblyName name)
        {
            var args = new ResolveEventArgs(name.FullName, requestingAssembly);
            return TryResolve(myDefaultResolver, args) ?? TryResolve(myNuGetResolver, args);
        }

        public IReadOnlyCollection<FileInfo> TryResolveOnly(Assembly requestingAssembly, AssemblyName name)
        {
            var args = new ResolveEventArgs(name.FullName, requestingAssembly);

            IEnumerable<T> TryResolveOnly<T>(IAssemblyResolver<T> resolver) where T : IAssemblyResolverData =>
                resolver.TryResolve(args, out var result) ? result : new List<T>();

            var results = TryResolveOnly(myDefaultResolver);
            var files = results
                .Select(x => x.File)
                .Concat(TryResolveOnly(myNuGetResolver).Select(x => x.File))
                .ToList();

            if (name.Name == "mscorlib" && name.Version == new Version(4, 0, 0, 0))
            {
                // C:\Windows\Microsoft.NET\Framework\v4.0.30319
                var netFwRoot = name.ProcessorArchitecture == ProcessorArchitecture.Amd64
                    ? @"%systemroot%\Microsoft.NET\Framework64"
                    : @"%systemroot%\Microsoft.NET\Framework";
                netFwRoot = Environment.ExpandEnvironmentVariables(netFwRoot);
                var version = Directory.GetDirectories(netFwRoot, "v4.0.*")
                    .Select(Path.GetFileName)
                    .OrderBy(x => x)
                    .Last();

                return new[] { new FileInfo(Path.Combine(netFwRoot, version, "mscorlib.dll")) };
            }

            return files;
        }
    }
}