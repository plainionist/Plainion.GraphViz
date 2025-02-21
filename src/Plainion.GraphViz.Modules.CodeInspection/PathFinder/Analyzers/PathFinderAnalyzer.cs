using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using Newtonsoft.Json;
using Plainion.GraphViz.Model;
using Plainion.GraphViz.Modules.CodeInspection.CallTree.Analyzers;
using Plainion.GraphViz.CodeInspection;
using Plainion.GraphViz.Modules.CodeInspection.Reflection;
using Plainion.GraphViz.Presentation;
using Plainion.Logging;
using Plainion.GraphViz.CodeInspection.AssemblyLoader;

namespace Plainion.GraphViz.Modules.CodeInspection.PathFinder.Analyzers
{
    class PathFinderAnalyzer
    {
        private static readonly ILogger myLogger = LoggerFactory.GetLogger(typeof(PathFinderAnalyzer));

        private class TypeNode
        {
            public string myId;
            public string myDisplayname;
            // through AppDomain.ApplyPolicy assemblies might not be unique any longer
            // so we take AssemblyName only (anyhow we are not interested in different versions here)
            public string myAssemblyName;
        }

        private class TypeNodeComparer : IEqualityComparer<TypeNode>
        {
            public bool Equals(TypeNode x, TypeNode y)
            {
                return x.myId == y.myId;
            }

            public int GetHashCode(TypeNode obj)
            {
                return obj.myId.GetHashCode();
            }
        }

        public bool KeepInnerAssemblyDependencies { get; set; }
        public bool KeepSourceAssemblyClusters { get; set; }
        public bool KeepTargetAssemblyClusters { get; set; }
        public bool AssemblyReferencesOnly { get; set; }

        private IEnumerable<Reference> GetUsedTypes(MonoLoader monoLoader, Type t)
        {
            Console.Write(".");

            var inspector = new Inspector(monoLoader, t);
            return inspector.GetUsedTypes();
        }

        private List<(Node, List<Reference>)> FindDependencies(MonoLoader monoLoader, Func<Type, bool> isRelevantType, List<(Node, List<Reference>)> analyzed, Node node)
        {
            var asm = monoLoader.Assemblies
                .Single(x => R.AssemblyName(x) == node.Id);

            var directDeps = asm.GetTypes()
                .AsParallel()
                .SelectMany(t => GetUsedTypes(monoLoader, t))
                .Where(r => isRelevantType(r.To))
                .ToList();

            var analyzedNodes = analyzed
                .Select(x => x.Item1.Id)
                .Distinct()
                .ToList();

            var newAnalyzed = analyzed.ToList();
            newAnalyzed.Add((node, directDeps));

            var indirectDeps = node.Out
                .Select(e => e.Target)
                .Where(d => !analyzedNodes.Contains(d.Id))
                .Aggregate(newAnalyzed, (acc, n) => FindDependencies(monoLoader, isRelevantType, acc, n))
                .ToList();

            indirectDeps.Add((node, directDeps));

            return indirectDeps;
        }

        private List<Reference> AnalyzeTypeDependencies(MonoLoader monoLoader, Func<Type, bool> isRelevantType, List<Node> sourceNodes)
        {
            return sourceNodes
                .Aggregate(new List<(Node, List<Reference>)>(), (acc, n) => FindDependencies(monoLoader, isRelevantType, acc, n))
                .SelectMany(x => x.Item2)
                .ToList();
        }

        private TypeNode TryCreateNode(Type t)
        {
            var t2 = t.GetCustomAttributesData().Any(d => d.AttributeType == typeof(CompilerGeneratedAttribute)) || t.IsGenericParameter ? t.DeclaringType : t;

            // can be null for generated code (e.g. ResourceManager)
            var nodeType = t2 ?? t;

            var id = R.TypeFullName(nodeType);

            // ignore CLR internals
            if (id.Contains("$") || id.Contains("@") || id.Contains("<PrivateImplementationDetails>"))
            {
                return null;
            }
            else
            {
                return new TypeNode
                {
                    myId = id,
                    myDisplayname = nodeType.Name,
                    myAssemblyName = R.AssemblyName(t.Assembly)
                };
            }
        }

        private IGraphPresentation CreateTypeDependencyGraph(IReadOnlyList<Assembly> sources, IReadOnlyList<Assembly> targets, IGraphPresentation assemblyGraphPresentation)
        {
            var relevantNodes = assemblyGraphPresentation.Graph.Nodes
                .Where(n => assemblyGraphPresentation.Picking.Pick(n))
                .ToList();

            var relevantAssemblies = new HashSet<string>(relevantNodes.Select(n => n.Id));

            var monoLoader = new MonoLoader(sources.Concat(targets));
            var sourceNodeIds = new HashSet<string>(sources.Select(s => R.AssemblyName(s)));
            var typeDependencies = AnalyzeTypeDependencies(
                monoLoader,
                t => relevantAssemblies.Contains(R.AssemblyName(t.Assembly)),
                relevantNodes.Where(n => sourceNodeIds.Contains(n.Id)).ToList());

            Console.WriteLine();
            Console.WriteLine("NOT analyzed assemblies:");

            foreach (var asm in monoLoader.SkippedAssemblies)
            {
                myLogger.Warning("  " + asm);
            }

            var builder = new RelaxedGraphBuilder();
            var edges = typeDependencies
                .Select(r => (TryCreateNode(r.From), TryCreateNode(r.To)))
                .Where(x => x.Item1 == null || x.Item2 == null || x.Item1 == x.Item2 ? false : true)
                // ignore those - would generate too much false-positives as namespace is often missing
                .Where(x => !x.Item1.myDisplayname.StartsWith("<>f__AnonymousType") && !x.Item2.myDisplayname.StartsWith("<>f__AnonymousType"))
                .Where(x => KeepInnerAssemblyDependencies || x.Item1.myAssemblyName != x.Item2.myAssemblyName)
                .ToList();

            foreach (var edge in edges)
            {
                builder.TryAddEdge(edge.Item1.myId, edge.Item2.myId);
            }

            // add clusters 
            var nodes = edges
                .SelectMany(e => new[] { e.Item1, e.Item2 })
                .Distinct(new TypeNodeComparer())
                .ToList();

            var sourceNodes = new List<TypeNode>();
            if (!KeepSourceAssemblyClusters)
            {
                sourceNodes.AddRange(nodes.Where(n => sources.Any(asm => R.AssemblyName(asm) == n.myAssemblyName)));

                builder.TryAddCluster("SOURCE", sourceNodes.Select(n => n.myId));
            }

            var targetNodes = new List<TypeNode>();
            if (!KeepTargetAssemblyClusters)
            {
                targetNodes.AddRange(nodes.Where(n => targets.Any(asm => R.AssemblyName(asm) == n.myAssemblyName)));

                builder.TryAddCluster("TARGET", targetNodes.Select(n => n.myId));
            }

            // add cluster for each assembly which is neither source nor target
            var alreadyClustered = sourceNodes
                .Concat(targetNodes)
                .ToList();

            var newClusters = nodes
                .Except(alreadyClustered)
                .GroupBy(n => n.myAssemblyName)
                .Select(x => (x.Key, x.Select(n => n.myId).ToList()));
            foreach (var cluster in newClusters)
            {
                builder.TryAddCluster(cluster.Item1, cluster.Item2);
            }

            var presentation = new GraphPresentation();
            presentation.Graph = builder.Graph;

            // add captions for readability
            var captions = presentation.GetModule<ICaptionModule>();
            foreach (var n in nodes)
            {
                captions.Add(new Caption(n.myId, n.myDisplayname));
            }

            return presentation;
        }

        private void Execute(IAssemblyLoader loader, IEnumerable<string> sourceAssemblies, IEnumerable<string> targetAssemblies, IEnumerable<string> relevantAssemblies, string outputFile)
        {
            Console.WriteLine("Source assemblies:");
            foreach (var asm in sourceAssemblies)
            {
                Console.WriteLine($"  {asm}");
            }

            Console.WriteLine("Target assemblies:");
            foreach (var asm in targetAssemblies)
            {
                Console.WriteLine($"  {asm}");
            }

            Console.WriteLine("Loading assemblies ...");
            var sources = sourceAssemblies.Select(asm => loader.TryLoadAssembly(asm)).Where(x => x != null).ToList();
            var targets = targetAssemblies.Select(asm => loader.TryLoadAssembly(asm)).Where(x => x != null).ToList();

            Console.WriteLine("Analyzing assembly dependencies ...");
            var analyzer = new AssemblyDependencyAnalyzer(loader, relevantAssemblies);
            var assemblyGraphPresentation = analyzer.CreateAssemblyGraph(sources, targets);

            if (AssemblyReferencesOnly)
            {
                GraphUtils.Serialize(outputFile, assemblyGraphPresentation);
            }
            else
            {
                Console.WriteLine("Analyze type dependencies ...");

                var p = CreateTypeDependencyGraph(sources, targets, assemblyGraphPresentation);
                GraphUtils.Serialize(outputFile, p);
            }
        }

        public void Execute(string configFile, bool assemblyReferencesOnly, string outputFile)
        {
            var definition = new
            {
                NetFramework = false,
                BinFolder = "",
                KeepInnerAssemblyDependencies = true,
                KeepSourceAssemblyClusters = false,
                KeepTargetAssemblyClusters = false,
                Sources = new[] { "" },
                Targets = new[] { "" },
                RelevantAssemblies = new[] { "" }
            };

            using (var reader = new StreamReader(configFile))
            {
                var config = JsonConvert.DeserializeAnonymousType(reader.ReadToEnd(), definition);

                var sources = ResolveAssemblies(config.BinFolder, config.Sources).ToList();
                var targets = ResolveAssemblies(config.BinFolder, config.Targets).ToList();

                KeepInnerAssemblyDependencies = config.KeepInnerAssemblyDependencies;
                KeepSourceAssemblyClusters = config.KeepSourceAssemblyClusters;
                KeepTargetAssemblyClusters = config.KeepTargetAssemblyClusters;
                AssemblyReferencesOnly = assemblyReferencesOnly;

                var loader = AssemblyLoaderFactory.Create(config.NetFramework ? DotNetRuntime.Framework : DotNetRuntime.Core);
                Execute(loader, sources, targets, config.RelevantAssemblies, outputFile);
            }
        }

        private static IEnumerable<string> ResolveAssemblies(string binFolder, string pattern)
        {
            var files = Directory.GetFiles(binFolder, pattern);
            if (files.Length == 0)
            {
                myLogger.Warning($"No assemblies found for pattern: {pattern}");
                return Enumerable.Empty<string>();
            }
            else
            {
                return files
                    .Select(f => Path.GetFullPath(f))
                    .ToList();
            }
        }

        private static IEnumerable<string> ResolveAssemblies(string binFolder, IEnumerable<string> patterns)
        {
            return patterns.SelectMany(p => ResolveAssemblies(binFolder, p)).ToList();
        }
    }
}

