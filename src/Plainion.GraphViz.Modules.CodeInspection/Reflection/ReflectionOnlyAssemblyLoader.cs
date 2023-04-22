using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using Plainion.Logging;

namespace Plainion.GraphViz.Modules.CodeInspection.Reflection
{
    internal class ReflectionOnlyAssemblyLoader : IAssemblyLoader
    {
        private static readonly ILogger myLogger = LoggerFactory.GetLogger(typeof(ReflectionOnlyAssemblyLoader));

        private readonly Dictionary<string, Assembly> myAssemblyCache;
        private readonly CustomMetadataAssemblyResolver myResolver;
        private MetadataLoadContext myContext;
        private Assembly myRequestingAssembly;

        public ReflectionOnlyAssemblyLoader()
        {
            myResolver = new CustomMetadataAssemblyResolver(() => myRequestingAssembly, typeof(object).Assembly);
            myContext = new MetadataLoadContext(myResolver, typeof(object).Assembly.GetName().Name);

            myAssemblyCache = new Dictionary<string, Assembly>();
            ForceLoadDependencies = true;

            //System.Diagnostics.Debugger.Launch();
        }

        public void Dispose()
        {
            myContext?.Dispose();
            myContext = null;

            myAssemblyCache.Clear();
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

                var oldRequestingAssembly = myRequestingAssembly;
                try
                {
                    myRequestingAssembly = requestingAssembly;

                    myResolver.AddAssembliesFromFolder(new FileInfo(requestingAssembly.Location).Directory);

                    assembly = myContext.LoadFromAssemblyName(dependency);
                    myAssemblyCache[dependency.FullName] = assembly;

                    if (assembly != null)
                    {
                        ForceLoadDependenciesIfRequested(assembly);
                    }

                    return assembly;
                }
                catch (Exception ex)
                {
                    myLogger.Warning($"Failed to load dependency {dependency}{Environment.NewLine}Reason: {ex.Message}");

                    // don't try loading again
                    myAssemblyCache[dependency.FullName] = null;

                    return null;
                }
                finally
                {
                    myRequestingAssembly = oldRequestingAssembly;
                }
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
                    myLogger.Debug($"Loading {path}");

                    myResolver.AddAssembliesFromFolder(new FileInfo(path).Directory);

                    assembly = myContext.LoadFromAssemblyPath(path);
                    myAssemblyCache[path] = assembly;

                    ForceLoadDependenciesIfRequested(assembly);

                    return assembly;
                }
                catch (Exception ex)
                {
                    myLogger.Error($"Failed to load assembly {path}{Environment.NewLine}Reason: {ex.Message}");

                    // don't try loading again
                    myAssemblyCache[path] = null;
                    return null;
                }
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

            var deps = asm.GetReferencedAssemblies();
            foreach (var dep in deps)
            {
                TryLoadDependency(asm, dep);
            }

            // get all types to ensure that all relevant assemblies are loaded while possible AssemblyResolve
            // event handlers are still registered.
            // no purpose in catching exception here - it will anyhow fail later on again when we try
            // to get the types for working with those.
            asm.GetTypes()
                .SelectMany(x => x.GetMembers());
        }
    }
}
