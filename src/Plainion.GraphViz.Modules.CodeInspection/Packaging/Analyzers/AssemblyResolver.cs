using System;
using System.IO;
using System.Reflection;
using Nuclear.Assemblies;
using Nuclear.Assemblies.Factories;
using Nuclear.Assemblies.ResolverData;
using Nuclear.Assemblies.Resolvers;
using Nuclear.Creation;
using Plainion.Logging;

namespace Plainion.GraphViz.Modules.CodeInspection.Packaging.Analyzers
{
    class AssemblyResolver : IDisposable
    {
        private static readonly ILogger myLogger = LoggerFactory.GetLogger(typeof(PackageAnalyzer));

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

        private Assembly TryResolve<T>(IAssemblyResolver<T> resolver, ResolveEventArgs args) where T : IAssemblyResolverData
        {
            if (resolver.TryResolve(args, out var result))
            {
                foreach (var item in result)
                {
                    if (AssemblyHelper.TryLoadFrom(item.File, out Assembly assembly))
                    {
                        return assembly;
                    }
                }
            }

            return null;
        }

    }
}
