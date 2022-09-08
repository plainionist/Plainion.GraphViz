using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Windows.Forms;
using Akka.Util;
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

        public Assembly TryResolve(AssemblyName name) =>
            TryResolve(myDefaultResolver, name) ?? TryResolve(myNuGetResolver, name);

        private Assembly TryResolve<T>(IAssemblyResolver<T> resolver, AssemblyName args) where T : IAssemblyResolverData =>
            resolver.TryResolve(args, out var result) ? TryLoad(result) : null;
    }
}
