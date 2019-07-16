using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;
using Plainion.GraphViz.Modules.CodeInspection.Core;
using Plainion.GraphViz.Modules.CodeInspection.Packaging.Spec;

namespace Plainion.GraphViz.Modules.CodeInspection.Packaging.Analyzers
{
    class PackageAnalyzer
    {
        private static string[] Colors = { "LightBlue", "LightGreen", "LightCoral", "Brown", "DarkTurquoise", "MediumAquamarine", "Orange",
                                           "LawnGreen","DarkKhaki","BurlyWood","SteelBlue","Goldenrod", "Tomato","Crimson","CadetBlue" };

        private SystemPackaging myConfig;
        private CancellationToken myCancellationToken;
        private readonly MonoLoader myAssemblyLoader;
        private Dictionary<string, List<Type>> myPackageToTypesMap;
        private List<Package> myRelevantPackages;

        public PackageAnalyzer()
        {
            myAssemblyLoader = new MonoLoader();
            PackagesToAnalyze = new List<string>();
        }

        /// <summary>
        /// If empty the dependencies between all packages will be analyzed
        /// </summary>
        public IList<string> PackagesToAnalyze { get; private set; }

        public bool UsedTypesOnly { get; set; }

        /// <summary>
        /// If no matching cluster was found for a node it will be put in a cluster for its namespace
        /// </summary>
        public bool CreateClustersForNamespaces { get; set; }

        public AnalysisDocument Execute(SystemPackaging config, CancellationToken cancellationToken)
        {
            myConfig = config;
            myCancellationToken = cancellationToken;

            myRelevantPackages = PackagesToAnalyze.Any()
                ? myConfig.Packages
                    .Where(p => PackagesToAnalyze.Any(name => p.Name.Equals(name, StringComparison.OrdinalIgnoreCase)))
                    .ToList()
                : myConfig.Packages;

            myPackageToTypesMap = new Dictionary<string, List<Type>>();

            Load();

            Console.WriteLine("Analyzing ...");

            var edges = Analyze()
                .Distinct()
                .ToList();

            Console.WriteLine();

            if (myAssemblyLoader.SkippedAssemblies.Any())
            {
                Console.WriteLine("Skipped assemblies:");
                foreach (var asm in myAssemblyLoader.SkippedAssemblies)
                {
                    Console.WriteLine("  {0}", asm);
                }
                Console.WriteLine();
            }

            Console.WriteLine("Building Graph ...");

            return GenerateDocument(edges);
        }

        private void Load()
        {
            foreach (var package in myRelevantPackages)
            {
                myCancellationToken.ThrowIfCancellationRequested();

                myPackageToTypesMap[package.Name] = Load(package)
                    .SelectMany(asm =>
                   {
                       try
                       {
                           return asm.GetTypes();
                       }
                       catch (ReflectionTypeLoadException ex)
                       {
                           Console.WriteLine("WARNING: not all types could be loaded from assembly {0}. Error: {1}{2}", asm.Location,
                               Environment.NewLine, ex.Dump());
                           return ex.Types.Where(t => t != null);
                       }
                   })
                    .ToList();
            }
        }

        private IEnumerable<Assembly> Load(Package package)
        {
            Console.WriteLine("Assembly root {0}", Path.GetFullPath(myConfig.AssemblyRoot));
            Console.WriteLine("Loading package {0}", package.Name);

            return package.Includes
                .SelectMany(i => Directory.GetFiles(myConfig.AssemblyRoot, i.Pattern))
                .Where(file => !package.Excludes.Any(e => e.Matches(file)))
                .Select(Load)
                .Where(asm => asm != null)
                .ToList();
        }

        private static Assembly Load(string path)
        {
            try
            {
                Console.WriteLine("Loading {0}", path);

                return Assembly.LoadFrom(path);
            }
            catch
            {
                Console.WriteLine("ERROR: failed to load assembly {0}", path);
                return null;
            }
        }

        private Reference[] Analyze()
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
                .SelectMany(e => Analyze(e.Package, e.Type))
                .ToArray();
        }

        private IEnumerable<Reference> Analyze(Package package, Type type)
        {
            Console.Write(".");

            myCancellationToken.ThrowIfCancellationRequested();

            return new Inspector(myAssemblyLoader, type).GetUsedTypes()
                .Where(edge => myPackageToTypesMap.Any(e => e.Value.Contains(edge.To)))
                .Select(edge => GraphUtils.Edge(edge))
                .Where(edge => edge.From != edge.To);
        }

        private AnalysisDocument GenerateDocument(IReadOnlyCollection<Reference> edges)
        {
            var doc = new AnalysisDocument();

            var nodesWithEdgesIndex = new HashSet<Type>();
            if (UsedTypesOnly)
            {
                foreach (var edge in edges)
                {
                    nodesWithEdgesIndex.Add(edge.From);
                    nodesWithEdgesIndex.Add(edge.To);
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
                .Where(e => !UsedTypesOnly || nodesWithEdgesIndex.Contains(e.Type))
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

            foreach (var edge in edges)
            {
                doc.Add(edge);

                var color = GetEdgeColor(edge);
                if (color != null)
                {
                    doc.AddEdgeColor(edge, color);
                }
            }

            return doc;
        }

        private Cluster GetCluster(Package package, Type type)
        {
            // if there is no namespace (compiler generated types with "<>") then "best match" would be the assembly name
            // to get the clustering as good as possible
            var fullName = type.Namespace != null ? type.FullName : type.Assembly.GetName().Name;

            var cluster = package.Clusters.FirstOrDefault(c => c.Matches(fullName));
            if (cluster != null)
            {
                return cluster;
            }

            if (package.CreateClustersForAssemblies)
            {
                // ID needs to be derived from assembly name to auto-magically match the next type 
                // of the same assembly into this cluster later on when building the graph
                return new Cluster { Name = type.Assembly.GetName().Name, Id = type.Assembly.FullName };
            }

            if (CreateClustersForNamespaces)
            {
                // ID needs to be derived from namespace name to auto-magically match the next type 
                // of the same namespace into this cluster later on when building the graph
                var id = type.Namespace ?? type.AssemblyQualifiedName;
                return new Cluster { Name = id, Id = id };
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
