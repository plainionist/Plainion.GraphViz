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
            myResolver = new CustomAssemblyResolver(typeof(object).Assembly);
            myContext = new MetadataLoadContext(myResolver, typeof(object).Assembly.GetName().Name);

            myAssemblyCache = new Dictionary<string, Assembly>();
            ForceLoadDependencies = true;

            System.Diagnostics.Debugger.Launch();
        }

        private class CustomAssemblyResolver : MetadataAssemblyResolver
        {
            private readonly HashSet<string> myFolders;
            // we must not add same assembly from different paths to PathAssemblyResolver.
            // it will fail with exception if version isnt exactly same
            private readonly Dictionary<string, string> myAssemblies;

            public CustomAssemblyResolver(Assembly coreAssembly)
            {
                myAssemblies = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
                myFolders = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

                myAssemblies.Add(Path.GetFileName(coreAssembly.Location), coreAssembly.Location);
            }

            public override Assembly Resolve(MetadataLoadContext context, AssemblyName assemblyName)
            {
                var assembly = new PathAssemblyResolver(myAssemblies.Values).Resolve(context, assemblyName);
                if (assembly != null)
                {
                    return assembly;
                }

                if (assemblyName.Name == "mscorlib" && assemblyName.Version == new Version(4, 0, 0, 0))
                {
                    var netFwRoot = assemblyName.ProcessorArchitecture == ProcessorArchitecture.Amd64
                        ? @"%systemroot%\Microsoft.NET\Framework64"
                        : @"%systemroot%\Microsoft.NET\Framework";
                    netFwRoot = Environment.ExpandEnvironmentVariables(netFwRoot);
                    var version = Directory.GetDirectories(netFwRoot, "v4.0.*")
                        .Select(Path.GetFileName)
                        .OrderBy(x => x)
                        .Last();

                    AddAssembliesFromFolder(Path.Combine(netFwRoot, version));

                    // .Net FW detected - add reference assemblies as well
                    AddAssembliesFromFolder(@"C:\Program Files (x86)\Reference Assemblies\Microsoft\Framework\.NETFramework\v4.8");

                    return context.LoadFromAssemblyPath(Path.Combine(netFwRoot, version, "mscorlib.dll"));
                }

                // try resolve .NET Core/6 and NuGet

                var resolve = new AssemblyResolver();

                var files = resolve.TryResolveOnly(assemblyName);

                if (files.Count == 0)
                {
                    myLogger.Warning($"Dependency not found '{assemblyName}'");
                    return null;
                }

                if (files.Count > 1)
                {
                    myLogger.Warning($"Multiple matching assemblies found for '{assemblyName}':");
                    foreach (var file in files)
                    {
                        myLogger.Warning($"  {file.FullName}");
                    }
                }

                AddAssembliesFromFolders(files);

                return context.LoadFromAssemblyPath(files.First().FullName);
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

                void AddPaths(IEnumerable<string> paths)
                {
                    foreach (var path in paths)
                    {
                        var key = Path.GetFileName(path);
                        if (!myAssemblies.ContainsKey(key))
                        {
                            myAssemblies[key] = path;
                        }
                    }
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
