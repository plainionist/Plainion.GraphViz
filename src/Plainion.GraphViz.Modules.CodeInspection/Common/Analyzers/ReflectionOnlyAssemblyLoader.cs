using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using Plainion.Logging;

namespace Plainion.GraphViz.Modules.CodeInspection.Common.Analyzers
{
    internal class ReflectionOnlyAssemblyLoader : IAssemblyLoader
    {
        private static readonly ILogger myLogger = LoggerFactory.GetLogger(typeof(ReflectionOnlyAssemblyLoader));

        private readonly Dictionary<string, Assembly> myAssemblyCache;
        private CustomAssemblyResolver myResolver;
        private MetadataLoadContext myContext;

        public ReflectionOnlyAssemblyLoader()
        {
            var coreAssembly = typeof(object).Assembly;
            myResolver = new CustomAssemblyResolver(new string[] { coreAssembly.Location });
            myContext = new MetadataLoadContext(myResolver, coreAssemblyName: coreAssembly.GetName().Name);

            myAssemblyCache = new Dictionary<string, Assembly>();
            ForceLoadDependencies = true;

            //System.Diagnostics.Debugger.Launch();
        }

        private class CustomAssemblyResolver : MetadataAssemblyResolver
        {
            private readonly HashSet<string> myFolders;
            private readonly HashSet<string> myAssemblies;

            public CustomAssemblyResolver(IEnumerable<string> paths)
            {
                myAssemblies = new HashSet<string>(paths, StringComparer.OrdinalIgnoreCase);
                myFolders = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            }

            public override Assembly Resolve(MetadataLoadContext context, AssemblyName assemblyName)
            {
                return new PathAssemblyResolver(myAssemblies).Resolve(context, assemblyName);
            }

            internal void AddAssembliesFromFolder(string folder)
            {
                if (myFolders.Contains(folder))
                {
                    return;
                }

                AddPaths(Directory.GetFiles(folder, "*.dll"));
                AddPaths(Directory.GetFiles(folder, "*.exe"));

                myFolders.Add(folder);
            }

            private void AddPaths(IEnumerable<string> paths)
            {
                foreach (var path in paths)
                {
                    myAssemblies.Add(path);
                }
            }

            internal void AddAssembliesFromFolders(IReadOnlyCollection<FileInfo> files)
            {
                foreach (var dir in files.Select(x => x.DirectoryName))
                {
                    AddAssembliesFromFolder(dir);
                }
            }
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

                try
                {
                    myResolver.AddAssembliesFromFolder(Path.GetDirectoryName(requestingAssembly.Location));

                    var resolve = new AssemblyResolver();

                    var files = resolve.TryResolveOnly(requestingAssembly, dependency);

                    if (files.Count == 0)
                    {
                        myLogger.Warning($"Dependency not found '{dependency}':");
                        return null;
                    }

                    if (files.Count > 1)
                    {
                        myLogger.Warning($"Multiple matching assemblies found for '{dependency}':");
                        foreach (var file in files)
                        {
                            myLogger.Warning($"  {file.FullName}");
                        }
                    }

                    myResolver.AddAssembliesFromFolders(files);

                    assembly = TryLoadAssembly(files.First().FullName);
                    myAssemblyCache[dependency.FullName] = assembly;

                    if (assembly != null)
                    {
                        ForceLoadDependenciesIfRequested(assembly);
                    }

                    return assembly;
                }
                catch (Exception ex)
                {
                    myLogger.Error($"Failed to load assembly {dependency}{Environment.NewLine}{ex.Message}");

                    // don't try loading again
                    myAssemblyCache[dependency.FullName] = null;

                    return null;
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

                    myResolver.AddAssembliesFromFolder(Path.GetDirectoryName(path));

                    assembly = ReflectionOnlyLoadFrom(path);
                    myAssemblyCache[path] = assembly;

                    ForceLoadDependenciesIfRequested(assembly);

                    return assembly;
                }
                catch (Exception ex)
                {
                    myLogger.Error($"Failed to load assembly {path}{Environment.NewLine}{ex.Message}");

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

        private Assembly ReflectionOnlyLoadFrom(string file)
        {
            try
            {
                return myContext.LoadFromAssemblyPath(file);
            }
            catch (BadImageFormatException)
            {
                myLogger.Warning($"Loading skipped for native/mixed mode assembly: {file}");
                return null;
            }
        }
    }
}
