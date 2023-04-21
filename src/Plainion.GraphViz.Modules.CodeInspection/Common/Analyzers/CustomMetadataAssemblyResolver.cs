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
        private readonly HashSet<string> myFolders;
        // we must not add same assembly from different paths to PathAssemblyResolver.
        // it will fail with exception if version isnt exactly same
        private readonly Dictionary<string, string> myAssemblies;
        private readonly RelativePathResolver myRelativePathResolver;
        private readonly NugetResolver myNuGetResolver;

        public CustomMetadataAssemblyResolver(Assembly coreAssembly)
        {
            myAssemblies = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            myFolders = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

            myAssemblies.Add(Path.GetFileName(coreAssembly.Location), coreAssembly.Location);

            myRelativePathResolver = new RelativePathResolver(VersionMatchingStrategy.Exact, SearchOption.AllDirectories);
            myNuGetResolver = new NugetResolver(VersionMatchingStrategy.Exact, VersionMatchingStrategy.Exact);
        }

        public override Assembly Resolve(MetadataLoadContext context, AssemblyName assemblyName)
        {
            var assembly = new PathAssemblyResolver(myAssemblies.Values).Resolve(context, assemblyName);
            if (assembly != null)
            {
                return assembly;
            }

            var mscorlibResolver = new MscorlibResolver();
            var mscorlibResult = mscorlibResolver.TryResolve(assemblyName, Assembly.GetExecutingAssembly())
                .OrderByDescending(x => x.AssemblyName.Version)
                .FirstOrDefault();

            if (mscorlibResult != null)
            {
                AddAssembliesFromFolder(mscorlibResult.File);

                AddAssembliesFromFolder(mscorlibResult.ReferenceAssemblies);

                return context.LoadFromAssemblyPath(mscorlibResult.File.FullName);
            }

            // try resolve .NET Core/6 and NuGet

            var file = myRelativePathResolver.TryResolve(assemblyName, Assembly.GetExecutingAssembly())
                .OrderByDescending(x => x.AssemblyName.Version)
                .Select(x => x.File)
                .FirstOrDefault();
            if (file != null)
            {
                AddAssembliesFromFolder(file);

                return context.LoadFromAssemblyPath(file.FullName);
            }

            file = myNuGetResolver.TryResolve(assemblyName, Assembly.GetExecutingAssembly())
                .OrderByDescending(x => x.AssemblyName.Version)
                .Select(x => x.File)
                .FirstOrDefault();

            if (file != null)
            {
                AddAssembliesFromFolder(file);

                return context.LoadFromAssemblyPath(file.FullName);
            }

            return null;
        }

        internal void AddAssembliesFromFolder(FileInfo file) =>
            AddAssembliesFromFolder(file.Directory.FullName);

        internal void AddAssembliesFromFolder(DirectoryInfo file) =>
            AddAssembliesFromFolder(file.FullName);

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
    }
}
