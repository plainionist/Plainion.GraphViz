using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using Plainion.Logging;

namespace Plainion.GraphViz.Modules.CodeInspection.Packaging.Analyzers
{
    class TypesLoader
    {
        private static readonly ILogger myLogger = LoggerFactory.GetLogger(typeof(TypesLoader));

        private readonly HashSet<Assembly> myAssemblies;

        public TypesLoader()
        {
            myAssemblies = new HashSet<Assembly>();
        }

        public IReadOnlyCollection<Assembly> Assemblies => myAssemblies;

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

            myAssemblies.Add(assembly);

            return GetTypes(assembly);
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


