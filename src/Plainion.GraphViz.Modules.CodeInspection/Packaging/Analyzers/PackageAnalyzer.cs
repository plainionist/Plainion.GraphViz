using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using Plainion.GraphViz.Modules.CodeInspection.Core;
using Plainion.GraphViz.Modules.CodeInspection.Packaging.Spec;
using Plainion.GraphViz.Modules.CodeInspection.Reflection;
using Plainion.Logging;

namespace Plainion.GraphViz.Modules.CodeInspection.Packaging.Analyzers
{
    partial class PackageAnalyzer
    {
        private static readonly string[] Colors = { "LightBlue", "LightGreen", "LightCoral", "Brown", "DarkTurquoise", "MediumAquamarine", "Orange",
                                           "LawnGreen","DarkKhaki","BurlyWood","SteelBlue","Goldenrod", "Tomato","Crimson","CadetBlue" };

        private readonly ILogger myLogger = LoggerFactory.GetLogger(typeof(PackageAnalyzer));

        private SystemPackaging mySpec;
        private CancellationToken myCancellationToken;
        private Dictionary<string, List<Type>> myPackageToTypesMap;
        private List<Package> myRelevantPackages;
        private readonly TypesLoader myTypesLoader;

        public PackageAnalyzer(TypesLoader typesLoader)
        {
            myTypesLoader = typesLoader;

            PackagesToAnalyze = new List<string>();
        }

        /// <summary>
        /// If empty the dependencies between all packages will be analyzed
        /// </summary>
        public IList<string> PackagesToAnalyze { get; private set; }

        public AnalysisDocument Execute(SystemPackaging config, CancellationToken cancellationToken)
        {
            mySpec = config;
            myCancellationToken = cancellationToken;

            myRelevantPackages = PackagesToAnalyze.Any()
                ? mySpec.Packages
                    .Where(p => PackagesToAnalyze.Any(name => p.Name.Equals(name, StringComparison.OrdinalIgnoreCase)))
                    .ToList()
                : mySpec.Packages;

            myPackageToTypesMap = new Dictionary<string, List<Type>>();

            foreach (var package in myRelevantPackages)
            {
                myCancellationToken.ThrowIfCancellationRequested();

                myPackageToTypesMap[package.Name] = Load(package).ToList();
            }

            myLogger.Info("Analyzing ...");

            var monoLoader = new MonoLoader(myTypesLoader.Assemblies);

            var references = Analyze(monoLoader);

            if (monoLoader.SkippedAssemblies.Any())
            {
                myLogger.Notice("Skipped assemblies:");
                foreach (var asm in monoLoader.SkippedAssemblies)
                {
                    myLogger.Notice("  {0}", asm);
                }
            }

            myLogger.Info("Building Graph ...");

            return GenerateDocument(references);
        }

        private IEnumerable<Type> Load(Package package)
        {
            Contract.Requires(!string.IsNullOrEmpty(package.Name), "Package requires a name");

            myLogger.Info("Assembly root {0}", Path.GetFullPath(mySpec.AssemblyRoot));
            myLogger.Info("Loading package {0}", package.Name);

            return package.Includes
                .SelectMany(i => Directory.GetFiles(mySpec.AssemblyRoot, i.Pattern))
                .Where(file => !package.Excludes.Any(e => e.Matches(file)))
                .SelectMany(file => myTypesLoader.TryLoadAllTypes(file))
                .ToList();
        }

        // preserves duplicates as we want to compute weight of edges later
        private Reference[] Analyze(MonoLoader monoLoader)
        {
            try
            {
                return myRelevantPackages
                    .SelectMany(p => myPackageToTypesMap[p.Name]
                       .Select(t => new
                       {
                           Package = p,
                           Type = t
                       })
                    )
                    .AsParallel()
                    .WithCancellation(myCancellationToken)
                    .SelectMany(e => Analyze(monoLoader, e.Type))
                    .ToArray();
            }
            finally
            {
                Console.WriteLine();
            }
        }

        private IEnumerable<Reference> Analyze(MonoLoader monoLoader, Type type)
        {
            Console.Write(".");

            myCancellationToken.ThrowIfCancellationRequested();

            return new Inspector(monoLoader, type).GetUsedTypes()
                .Where(edge => myPackageToTypesMap.Any(e => e.Value.Contains(edge.To)))
                .Select(edge => GraphUtils.Edge(edge))
                .Where(edge => edge.From != edge.To);
        }

        private AnalysisDocument GenerateDocument(IReadOnlyCollection<Reference> references)
        {
            var doc = new AnalysisDocument();

            var nodesWithEdgesIndex = new HashSet<Type>();
            if (mySpec.UsedTypesOnly)
            {
                foreach (var reference in references)
                {
                    nodesWithEdgesIndex.Add(reference.From);
                    nodesWithEdgesIndex.Add(reference.To);
                }
            }

            var relevantNotesWithCluster = myPackageToTypesMap
                .Select(e => new
                {
                    Package = myRelevantPackages.Single(p => p.Name == e.Key),
                    Types = e.Value
                })
                .SelectMany((e, idx) => e.Types
                   .Select(t => new
                   {
                       Type = t,
                       Package = e.Package,
                       PackageIndex = idx
                   }))
                .AsParallel()
                .Where(e => !mySpec.UsedTypesOnly || nodesWithEdgesIndex.Contains(e.Type))
                .Select(e => new
                {
                    Node = e.Type,
                    Cluster = GetCluster(e.Package, e.Type),
                    PackageIndex = e.PackageIndex
                });

            foreach (var entry in relevantNotesWithCluster)
            {
                doc.Add(entry.Node);

                if (entry.Cluster != null)
                {
                    doc.AddToCluster(entry.Node, entry.Cluster);
                }

                if (myPackageToTypesMap.Count > 1)
                {
                    // color coding of nodes we only need if multiple packages were analyzed
                    doc.AddNodeColor(entry.Node, Colors[entry.PackageIndex % Colors.Length]);
                }
            }

            foreach (var reference in references)
            {
                doc.Add(reference);

                var color = GetEdgeColor(reference);
                if (color != null)
                {
                    doc.AddEdgeColor(reference, color);
                }
            }

            return doc;
        }

        private Cluster GetCluster(Package package, Type type)
        {
            // if there is no namespace (compiler generated types with "<>") then "best match" would be the assembly name
            // to get the clustering as good as possible
            var fullName = type.Namespace != null ? type.FullName : type.Assembly.GetName().Name;

            // For AutoClusters:
            // ID needs to be derived from assembly/namespace to auto-magically match the next type 
            // of the same assembly/namespace into this cluster later on when building the graph

            if ("assembly".Equals(package.AutoClusters, StringComparison.OrdinalIgnoreCase))
            {
                return new Cluster { Name = type.Assembly.GetName().Name, Id = type.Assembly.FullName };
            }

            if ("namespace".Equals(package.AutoClusters, StringComparison.OrdinalIgnoreCase))
            {
                var id = type.Namespace ?? type.Assembly.FullName;
                return new Cluster { Name = id, Id = id };
            }

            if (package.AutoClusters != null && package.AutoClusters.StartsWith("rootnamespace+", StringComparison.OrdinalIgnoreCase))
            {
                var assemblyName = type.Assembly.GetName().Name;
                if (!type.Namespace.StartsWith(assemblyName))
                {
                    return new Cluster { Name = assemblyName, Id = assemblyName };
                }

                var tokens = type.Namespace.Split('.');
                var numSubNamespaces = Int32.Parse(package.AutoClusters.Split('+').Last());
                var id = string.Join(".", tokens.Take(Math.Min(assemblyName.Split('.').Length + numSubNamespaces, tokens.Length)));
                return new Cluster { Name = id, Id = id };
            }

            var cluster = package.Clusters.FirstOrDefault(c => c.Matches(fullName));
            if (cluster != null)
            {
                return cluster;
            }

            return null;
        }

        private static string GetEdgeColor(Reference edge)
        {
            if (edge.ReferenceType == ReferenceType.DerivesFrom || edge.ReferenceType == ReferenceType.Implements)
            {
                return "Blue";
            }
            else if (edge.ReferenceType != ReferenceType.Calls)
            {
                return "Gray";
            }
            return null;
        }
    }
}
