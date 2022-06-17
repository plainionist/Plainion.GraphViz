using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using Nuclear.Assemblies;
using Nuclear.Assemblies.Factories;
using Nuclear.Assemblies.Resolvers;
using Nuclear.Creation;
using Plainion.Logging;

namespace Plainion.GraphViz.Modules.CodeInspection.Packaging.Analyzers
{
    class TypesLoader : IDisposable
    {
        private readonly ILogger myLogger = LoggerFactory.GetLogger(typeof(PackageAnalyzer));

        public TypesLoader()
        {
            AppDomain.CurrentDomain.AssemblyResolve += OnAssemblyResolve;
        }

        public void Dispose()
        {
            AppDomain.CurrentDomain.AssemblyResolve -= OnAssemblyResolve;
        }

        public IEnumerable<Type> TryLoadAllTypes(string path)
        {
            if (!IsAssembly(path))
            {
                return Enumerable.Empty<Type>();
            }

            var assembly = TryLoadAssembly(path);
            if (assembly == null)
            {
                return Enumerable.Empty<Type>();
            }

            return GetTypes(assembly);
        }

        private Assembly OnAssemblyResolve(object sender, ResolveEventArgs args)
        {
            myLogger.Info($"Trying to resolve: {args.Name}");

            {
                Factory.Instance.DefaultResolver().Create(out var resolver, VersionMatchingStrategies.Strict, SearchOption.AllDirectories);

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
            }

            {
                Factory.Instance.NugetResolver().Create(out var resolver, VersionMatchingStrategies.Strict, VersionMatchingStrategies.Strict);

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
            }

            return null;
        }

        private static bool IsAssembly(string path)
        {
            var ext = Path.GetExtension(path);
            return ".dll".Equals(ext, StringComparison.OrdinalIgnoreCase) || ".exe".Equals(ext, StringComparison.OrdinalIgnoreCase);
        }

        private Assembly TryLoadAssembly(string path)
        {
            try
            {
                myLogger.Info("Loading {0}", path);

                return Assembly.LoadFrom(path);
            }
            catch (Exception ex)
            {
                myLogger.Error($"Failed to load assembly {path}{Environment.NewLine}{ex.Message}");
                return null;
            }
        }

        private IEnumerable<Type> GetTypes(Assembly assembly)
        {
            IEnumerable<Type> TryGetAllTypes(Assembly assembly)
            {
                try
                {
                    return assembly.GetTypes();
                }
                catch (ReflectionTypeLoadException ex)
                {
                    myLogger.Warning("Not all types could be loaded from assembly {0}. Error: {1}{2}",
                        assembly.Location, Environment.NewLine, ex.Dump());

                    return ex.Types
                        .Where(t => t != null);
                }
            }

            bool IsAnalyzable(Type type)
            {
                try
                {
                    // even if we get a type from "Assembly.GetTypes" or from
                    // "ReflectionTypeLoadException.Types" it might still not be analyzable 
                    // as it throws exception when accessing e.g. Namespace property
                    return type != null && type.Namespace != null;
                }
                catch
                {
                    // for some strange reason, in such a case, we can safely access "FullName" but
                    // will get exception from "Namespace"
                    myLogger.Warning($"Failed to load '{type.FullName}'");
                    return false;
                }
            }

            return TryGetAllTypes(assembly)
                .Where(IsAnalyzable);
        }
    }
}


