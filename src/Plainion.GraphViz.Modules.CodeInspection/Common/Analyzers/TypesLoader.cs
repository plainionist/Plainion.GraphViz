using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Plainion.Logging;

namespace Plainion.GraphViz.Modules.CodeInspection.Common.Analyzers
{
    class TypesLoader
    {
        private static readonly ILogger myLogger = LoggerFactory.GetLogger(typeof(TypesLoader));

        private readonly AssemblyLoader myAssemblyLoader;
        private readonly HashSet<Assembly> myAssemblies;

        public TypesLoader()
        {
            myAssemblyLoader = new AssemblyLoader();
            myAssemblies = new HashSet<Assembly>();
        }

        public IReadOnlyCollection<Assembly> Assemblies => myAssemblies;

        public IEnumerable<Type> TryLoadAllTypes(string path)
        {
            var assembly = myAssemblyLoader.TryLoadAssembly(path);
            if (assembly == null)
            {
                return Enumerable.Empty<Type>();
            }

            myAssemblies.Add(assembly);

            return GetTypes(assembly);
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


