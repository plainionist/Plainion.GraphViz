using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using Plainion.Logging;

namespace Plainion.GraphViz.Modules.CodeInspection.Common.Analyzers
{
    class FullAssemblyLoader : IAssemblyLoader
    {
        private static readonly ILogger myLogger = LoggerFactory.GetLogger(typeof(FullAssemblyLoader));

        private readonly Dictionary<string, Assembly> myAssemblyCache;

        public FullAssemblyLoader()
        {
            myAssemblyCache = new Dictionary<string, Assembly>();
        }

        public bool ForceLoadDependencies { get; set; }

        public Assembly TryLoadDependency(Assembly requestingAssembly, AssemblyName dependency)
        {
            lock (myAssemblyCache)
            {
                if (myAssemblyCache.TryGetValue(dependency.FullName, out Assembly assembly))
                {
                    return assembly;
                }

                try
                {
                    var resolve = new AssemblyResolver();

                    assembly = resolve.TryResolve(requestingAssembly, dependency);
                    if (assembly != null)
                    {
                        ForceLoadDependenciesIfRequested(assembly);
                    }
                }
                catch (Exception ex)
                {
                    myLogger.Error($"Failed to load assembly {dependency}{Environment.NewLine}{ex.Message}");
                }

                myAssemblyCache[dependency.FullName] = assembly;

                return assembly;
            }
        }

        public Assembly TryLoadAssembly(string path)
        {
            if (!IsAssembly(path))
            {
                return null;
            }

            lock (myAssemblyCache)
            {
                if (myAssemblyCache.TryGetValue(path, out Assembly assembly))
                {
                    return assembly;
                }

                try
                {
                    assembly = Assembly.LoadFrom(path);

                    ForceLoadDependenciesIfRequested(assembly);
                }
                catch (Exception ex)
                {
                    myLogger.Error($"Failed to load assembly {path}{Environment.NewLine}{ex.Message}");
                }

                myAssemblyCache[path] = assembly;

                return assembly;
            }
        }

        private static bool IsAssembly(string path)
        {
            var ext = Path.GetExtension(path);
            return ".dll".Equals(ext, StringComparison.OrdinalIgnoreCase) || ".exe".Equals(ext, StringComparison.OrdinalIgnoreCase);
        }

        private void ForceLoadDependenciesIfRequested(Assembly asm)
        {
            if (!ForceLoadDependencies)
            {
                return;
            }

            // get all types to ensure that all relevant assemblies are loaded while possible AssemblyResolve
            // event handlers are still registered.
            // no purpose in catching exception here - it will anyhow fail later on again when we try
            // to get the types for working with those.
            asm.GetTypes();
        }
    }
}
