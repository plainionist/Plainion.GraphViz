using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using Plainion.Logging;

namespace Plainion.GraphViz.Modules.CodeInspection.Reflection
{
    class CustomMetadataAssemblyResolver : MetadataAssemblyResolver
    {
        private static readonly ILogger myLogger = LoggerFactory.GetLogger(typeof(CustomMetadataAssemblyResolver));

        private readonly HashSet<string> myFolders;
        // we must not add same assembly from different paths to PathAssemblyResolver.
        // it will fail with exception if version isnt exactly same
        private readonly Dictionary<string, string> myAssemblies;
        private readonly MscorlibResolver myMscorlibResolver;
        private readonly GacResolver myGacResolver;
        private readonly RelativePathResolver myRelativePathResolver;
        private readonly NugetResolver myNuGetResolver;
        private readonly Func<Assembly> myTryGetRequestingAssembly;

        public CustomMetadataAssemblyResolver(Func<Assembly> tryGetRequestingAssembly, Assembly coreAssembly)
        {
            Contract.RequiresNotNull(tryGetRequestingAssembly, nameof(tryGetRequestingAssembly));

            myTryGetRequestingAssembly = tryGetRequestingAssembly;
            myAssemblies = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            myFolders = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

            myAssemblies.Add(Path.GetFileName(coreAssembly.Location), coreAssembly.Location);

            myMscorlibResolver = new MscorlibResolver();
            myRelativePathResolver = new RelativePathResolver(VersionMatchingStrategy.SemanticVersion, SearchOption.AllDirectories);
            myGacResolver = new GacResolver();
            myNuGetResolver = new NugetResolver(VersionMatchingStrategy.SemanticVersion);

            System.Diagnostics.Debugger.Launch();
        }

        public override Assembly Resolve(MetadataLoadContext context, AssemblyName assemblyName)
        {
            var assembly = ResolveCore(context, assemblyName);
            myLogger.Debug($"{assembly} => {assembly?.Location}");
            return assembly;
        }

        private Assembly ResolveCore(MetadataLoadContext context, AssemblyName assemblyName)
        { 
            var requestingAssembly = myTryGetRequestingAssembly() ?? Assembly.GetEntryAssembly();

            var assembly = new PathAssemblyResolver(myAssemblies.Values).Resolve(context, assemblyName);
            if (assembly != null)
            {
                return assembly;
            }

            var mscorlibResult = myMscorlibResolver.TryResolve(assemblyName, requestingAssembly)
                .OrderByDescending(x => x.AssemblyName.Version)
                .FirstOrDefault();

            if (mscorlibResult != null)
            {
                AddAssembliesFromFolder(mscorlibResult.File.Directory);
                AddAssembliesFromFolder(mscorlibResult.ReferenceAssemblies);

                return context.LoadFromAssemblyPath(mscorlibResult.File.FullName);
            }

            var file = myRelativePathResolver.TryResolve(assemblyName, requestingAssembly)
                .OrderByDescending(x => x.AssemblyName.Version)
                .Select(x => x.File)
                .FirstOrDefault();

            if (file != null)
            {
                AddAssembliesFromFolder(file.Directory);

                return context.LoadFromAssemblyPath(file.FullName);
            }

            file = myGacResolver.TryResolve(assemblyName, requestingAssembly)
                .OrderByDescending(x => x.AssemblyName.Version)
                .Select(x => x.File)
                .FirstOrDefault();

            if (file != null)
            {
                return context.LoadFromAssemblyPath(file.FullName);
            }

            file = myNuGetResolver.TryResolve(assemblyName, requestingAssembly)
                .OrderByDescending(x => x.AssemblyName.Version)
                .Select(x => x.File)
                .FirstOrDefault();

            if (file != null)
            {
                return context.LoadFromAssemblyPath(file.FullName);
            }

            return null;
        }

        internal void AddAssembliesFromFolder(DirectoryInfo folder)
        {
            if (myFolders.Contains(folder.FullName))
            {
                return;
            }

            AddPaths(folder.GetFiles("*.dll").Select(x => x.FullName));
            AddPaths(folder.GetFiles("*.exe").Select(x => x.FullName));

            myFolders.Add(folder.FullName);

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
