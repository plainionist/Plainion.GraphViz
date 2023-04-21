using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using Plainion.Logging;

namespace Plainion.GraphViz.Modules.CodeInspection.Common.Analyzers
{
    class AssemblyResolver : IDisposable
    {
        private static readonly ILogger myLogger = LoggerFactory.GetLogger(typeof(AssemblyResolver));

        private readonly RelativePathResolver myRelativePathResolver;
        private readonly NugetResolver myNuGetResolver;

        public AssemblyResolver()
        {
            myRelativePathResolver = new RelativePathResolver(VersionMatchingStrategy.Exact, SearchOption.AllDirectories);
            myNuGetResolver = new NugetResolver(VersionMatchingStrategy.Exact, VersionMatchingStrategy.Exact);

            AppDomain.CurrentDomain.AssemblyResolve += OnAssemblyResolve;
        }

        public void Dispose()
        {
            AppDomain.CurrentDomain.AssemblyResolve -= OnAssemblyResolve;
        }

        private Assembly OnAssemblyResolve(object sender, ResolveEventArgs args)
        {
            myLogger.Info($"Trying to resolve: {args.Name}");

            return TryResolve(myRelativePathResolver, args) ?? TryResolve(myNuGetResolver, args);
        }

        private Assembly TryResolve<T>(AbstractAssemblyResolver<T> resolver, ResolveEventArgs args) where T : AssemblyResolutionResult =>
            TryLoad(resolver.TryResolve(new AssemblyName(args.Name), args.RequestingAssembly));

        private Assembly TryLoad<T>(IEnumerable<T> result) where T : AssemblyResolutionResult
        {
            foreach (var item in result)
            {
                var assembly = Assembly.LoadFrom(item.File.FullName);
                if (assembly != null)
                {
                    return assembly;
                }
            }
            return null;
        }

        public Assembly TryResolve(Assembly requestingAssembly, AssemblyName name)
        {
            var args = new ResolveEventArgs(name.FullName, requestingAssembly);
            return TryResolve(myRelativePathResolver, args) ?? TryResolve(myNuGetResolver, args);
        }

        public IReadOnlyCollection<FileInfo> TryResolveOnly(AssemblyName name)
        {
            IEnumerable<T> TryResolveOnly<T>(AbstractAssemblyResolver<T> resolver) where T : AssemblyResolutionResult =>
                resolver.TryResolve(name);

            var results = TryResolveOnly(myRelativePathResolver);
            return results
                .Select(x => x.File)
                .Concat(TryResolveOnly(myNuGetResolver).Select(x => x.File))
                .ToList();
        }
    }
}