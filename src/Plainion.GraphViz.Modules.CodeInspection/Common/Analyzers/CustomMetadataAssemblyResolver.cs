using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using Plainion.Logging;

namespace Plainion.GraphViz.Modules.CodeInspection.Common.Analyzers
{
    class CustomMetadataAssemblyResolver : MetadataAssemblyResolver
    {
        private static readonly ILogger myLogger = LoggerFactory.GetLogger(typeof(CustomMetadataAssemblyResolver));

        private readonly HashSet<string> myFolders;
        // we must not add same assembly from different paths to PathAssemblyResolver.
        // it will fail with exception if version isnt exactly same
        private readonly Dictionary<string, string> myAssemblies;

        public CustomMetadataAssemblyResolver(Assembly coreAssembly)
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
}
