using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Plainion.Graphs;
using Plainion.GraphViz.Actors.Client;
using Plainion.Graphs.Algorithms;
using Plainion.GraphViz.CodeInspection;
using Plainion.GraphViz.CodeInspection.AssemblyLoader;
using Plainion.GraphViz.Presentation;

namespace Plainion.GraphViz.Modules.CodeInspection.CallTree.Analyzers
{
    class TargetDescriptor
    {
        public string Assembly { get; set; }
        public string Type { get; set; }
        public string Method { get; set; }
    }

    class CallTreeAnalyzer
    {
        private readonly Microsoft.Extensions.Logging.ILoggerFactory myLoggerFactory;
        private readonly ILogger<CallTreeAnalyzer> myLogger;

        private class MethodNode
        {
            public string myId;
            public string myCaption;
            public Type myDeclaringType;
        }

        private class MethodDesc
        {
            public Type myDeclaringType;
            public string myName;
        }

        private class MethodNodeComparer : IEqualityComparer<MethodNode>
        {
            public bool Equals(MethodNode x, MethodNode y)
            {
                return x.myId == y.myId;
            }

            public int GetHashCode(MethodNode obj)
            {
                return obj.myId.GetHashCode();
            }
        }

        public CallTreeAnalyzer(Microsoft.Extensions.Logging.ILoggerFactory loggerFactory)
        {
            Contract.RequiresNotNull(loggerFactory, nameof(loggerFactory));

            myLoggerFactory = loggerFactory;
            myLogger = loggerFactory.CreateLogger<CallTreeAnalyzer>();
        }

        public bool AssemblyReferencesOnly { get; set; }
        public bool StrictCallsOnly { get; set; }

        private IEnumerable<MethodCall> GetCalledMethods(ILogger<Inspector> logger, MonoLoader monoLoader, Type t)
        {
            Console.Write(".");

            var inspector = new Inspector(logger, monoLoader, t);
            return inspector.GetCalledMethods();
        }

        private string MethodId(Type declaringType, string name)
        {
            return R.TypeFullName(declaringType) + "." + name;
        }

        private MethodNode CreateMethodNode(Method method)
        {
            return new MethodNode
            {
                myId = MethodId(method.DeclaringType, method.Name),
                myCaption = method.DeclaringType.Name + "." + method.Name,
                myDeclaringType = method.DeclaringType
            };
        }

        private bool CoveredByAssemblyGraph(IEnumerable<Node> relevantNodes, Type t)
        {
            return relevantNodes
                .Select(n => n.Id)
                .Contains(R.AssemblyName(t.Assembly));
        }

        private IEnumerable<string> GetAllMethods(Type t)
        {
            var methods = t.GetMethods().Select(m => m.Name);

            var properties = t.GetProperties()
                .SelectMany(p => new[] { p.GetGetMethod(), p.GetSetMethod() })
                .Where(m => m != null)
                .Select(m => m.Name);

            var events = t.GetEvents()
                .SelectMany(e => new[] { e.GetAddMethod(), e.GetRemoveMethod() })
                .Where(m => m != null)
                .Select(m => m.Name);

            return methods
                .Concat(properties)
                .Concat(events)
                .ToList();
        }

        private IEnumerable<MethodDesc> GetMethods(Assembly assembly, string typeName, string methodName)
        {
            var declaringType = assembly.GetTypes().Single(t => t.FullName == typeName);

            var methods = GetAllMethods(declaringType).Distinct();

            if (!methodName.Equals("*"))
            {
                methods = methods.Where(name => name == methodName);
            }

            if (methods == null || methods.Count() == 0)
            {
                throw new InvalidOperationException($"Method not found {typeName}.{methodName} in {assembly.FullName}");
            }

            return methods.Select(method => new MethodDesc
            {
                myDeclaringType = declaringType,
                myName = method
            });
        }

        private List<MethodCall> Analyze(ILogger<Inspector> inspectorLogger, MonoLoader monoLoader, IEnumerable<Node> relevantNodes, InterfaceImplementationsMap interfaceImplementationsMap, List<Type> analyzed, List<Type> callers)
        {
            var calls = callers.AsParallel()
                .SelectMany(t =>
                    GetCalledMethods(inspectorLogger, monoLoader, t)
                        .Where(r => CoveredByAssemblyGraph(relevantNodes, r.To.DeclaringType))
                        .SelectMany(x => interfaceImplementationsMap.ResolveInterface(x)))
                .ToList();

            var unknownTypes = calls.AsParallel()
                .Select(e => e.To.DeclaringType)
                .Distinct()
                .ToList()
                .Except(analyzed)
                .ToList();

            if (!unknownTypes.Any())
            {
                return calls;
            }
            else
            {
                var children = Analyze(inspectorLogger, monoLoader, relevantNodes, interfaceImplementationsMap, callers.Concat(analyzed).Distinct().ToList(), unknownTypes);
                return calls.Concat(children).ToList();
            }
        }

        // traces from the given source nodes all calls within the assembly graph
        private List<MethodCall> TraceCalles(ILogger<Inspector> inspectorLogger, MonoLoader monoLoader, IEnumerable<Node> relevantNodes, InterfaceImplementationsMap interfaceImplementationsMap, List<MethodDesc> targets, IEnumerable<Assembly> sources)
        {
            var targetTypes = targets
                .Select(m => m.myDeclaringType)
                .Distinct()
                .ToList();

            return Analyze(inspectorLogger, monoLoader, relevantNodes, interfaceImplementationsMap, targetTypes,
                sources.SelectMany(x => x.GetTypes()).ToList())
                .Distinct()
                .ToList();
        }

        private GraphPresentation BuildCallTree(List<Assembly> sources, List<MethodDesc> targets, GraphPresentation assemblyGraphPresentation)
        {
            var relevantNodes = assemblyGraphPresentation.Graph.Nodes
                .Where(n => assemblyGraphPresentation.Picking.Pick(n))
                .ToList();

            var assemblies = sources
                .Concat(targets.Select(x => x.myDeclaringType.Assembly))
                .ToList();

            var interfaceImplementationsMap = new InterfaceImplementationsMap(myLoggerFactory.CreateLogger<InterfaceImplementationsMap>(), assemblies);
            interfaceImplementationsMap.Build(relevantNodes, targets.Select(t => t.myDeclaringType));

            var monoLoader = new MonoLoader(assemblies);
            var inspectorLogger = myLoggerFactory.CreateLogger<Inspector>();
            var calls = TraceCalles(inspectorLogger, monoLoader, relevantNodes, interfaceImplementationsMap, targets, sources);

            Console.WriteLine();
            Console.WriteLine("NOT analyzed assemblies:");

            foreach (var x in monoLoader.SkippedAssemblies)
            {
                myLogger.LogWarning($"    {x}");
            }

            return Profile("Generating call graph ...", () =>
            {
                var builder = new RelaxedGraphBuilder();
                var edges = calls
                    .Select(call => (CreateMethodNode(call.From), CreateMethodNode(call.To)))
                    // we assume that usages within same class are not relevant
                    .Where(x => x.Item1.myDeclaringType != x.Item2.myDeclaringType)
                    .ToList();

                foreach (var edge in edges)
                {
                    builder.TryAddEdge(edge.Item1.myId, edge.Item2.myId);
                }

                var nodes = edges
                    .SelectMany(e => new[] { e.Item1, e.Item2 })
                    .Distinct(new MethodNodeComparer())
                    .ToList();

                var clusters = nodes
                    .GroupBy(n => n.myDeclaringType)
                    .Select(x => (R.TypeFullName(x.Key), x.Key.Name, x.Select(n => n.myId).ToList()))
                    .ToList();

                foreach (var cluster in clusters)
                {
                    builder.TryAddCluster(cluster.Item1, cluster.Item3);
                }

                builder.Freeze();

                var presentation = new GraphPresentation();
                presentation.Graph = builder.Graph;

                // add captions for readability
                var captions = presentation.GetModule<ICaptionModule>();

                foreach (var n in nodes)
                {
                    captions.Add(new Caption(n.myId, n.myCaption));
                }

                foreach (var cluster in clusters)
                {
                    captions.Add(new Caption(cluster.Item1, cluster.Item2));
                }

                return presentation;
            });
        }

        public static T Profile<T>(string caption, Func<T> f)
        {
            Console.WriteLine(caption);

            var watch = new Stopwatch();
            watch.Start();

            var ret = f();

            watch.Stop();
            Console.WriteLine($"{caption} elapsed {watch.Elapsed}");

            return ret;
        }

        private void CopyCaptions(GraphPresentation target, GraphPresentation source)
        {
            var t = target.GetPropertySetFor<Caption>();

            foreach (var c in source.GetPropertySetFor<Caption>().Items)
            {
                t.Add(c);
            }
        }

        // direct call dependencies only
        private GraphPresentation ReduceToDirectDependencies(List<MethodDesc> targets, GraphPresentation inputPresentation)
        {
            var presentation = new GraphPresentation();
            presentation.Graph = inputPresentation.Graph;

            CopyCaptions(presentation, inputPresentation);

            // find all nodes from which the targets can be reached
            var algo = new AddRemoveTransitiveHull(presentation);
            algo.Add = false;
            algo.Reverse = true;

            var targetIds = targets.Select(target => MethodId(target.myDeclaringType, target.myName)).ToList();
            var mask = algo.Compute(presentation.Graph.Nodes.AsParallel()
                .Where(n => targetIds.Contains(n.Id))
                .ToList());
            mask.Invert(presentation.TransformedGraph, presentation.Picking);

            presentation.Masks().Push(mask);

            return presentation;
        }

        // including type dependencies 
        private GraphPresentation ReduceToIndirectDependencies(List<MethodDesc> targets, GraphPresentation inputPresentation)
        {
            var presentation = new GraphPresentation();
            presentation.Graph = inputPresentation.Graph;

            CopyCaptions(presentation, inputPresentation);

            var transformations = presentation.GetModule<ITransformationModule>();

            // 1. find clusters containing targets
            var targetIds = targets.Select(target => MethodId(target.myDeclaringType, target.myName)).ToList();
            var targetClusters = targetIds
                .Select(targetId => transformations.Graph.Clusters
                    .AsParallel()
                    .SingleOrDefault(c => c.Nodes.Any(n => n.Id == targetId)))
                .Where(x => x != null)
                .ToList();

            // 2. fold all clusters to "see" type dependencies only
            presentation.ToogleFoldingOfVisibleClusters();

            // 3. unfold clusters of targets again to only "see" dependencies to target nodes
            foreach (var c in targetClusters)
            {
                presentation.ClusterFolding.Toggle(c.Id);
            }

            // 4. find all nodes from which the targets can be reached
            var algo = new AddRemoveTransitiveHull(presentation);
            algo.Add = false;
            algo.Reverse = true;

            var mask = algo.Compute(transformations.Graph.Nodes.AsParallel()
                .Where(n => targetIds.Contains(n.Id))
                .ToList());
            mask.Invert(presentation.TransformedGraph, presentation.Picking);

            presentation.Masks().Push(mask);

            return presentation;
        }

        private void Execute(IAssemblyLoader loader, IEnumerable<string> sourceAssemblies, IEnumerable<TargetDescriptor> targetDescriptors, IEnumerable<string> relevantAssemblies, string outputFile)
        {
            Console.WriteLine("Source assemblies:");
            foreach (var asm in sourceAssemblies)
            {
                Console.WriteLine($"  {asm}");
            }

            Console.WriteLine("Loading assemblies ...");
            var sources = sourceAssemblies
                .Select(asm => loader.TryLoadAssembly(asm))
                .Where(x => x != null)
                .ToList();

            var nestedTargets = Profile("Loading target methods ...", () =>
                targetDescriptors
                    .Select(target =>
                    {
                        var asm = loader.TryLoadAssembly(target.Assembly);
                        return (asm, GetMethods(asm, target.Type, target.Method));
                    })
                    .ToList());

            var targets = nestedTargets.SelectMany(t => t.Item2
                .Select(method =>
                {
                    return (t.Item1, method);
                }).ToList());

            Console.WriteLine("Targets:");
            foreach (var t in targets)
            {
                Console.WriteLine($"  {R.AssemblyName(t.Item1)}/{t.Item2.myDeclaringType}.{t.Item2.myName}");
            }

            var analyzer = new AssemblyDependencyAnalyzer(loader, relevantAssemblies);
            var assemblyGraphPresentation = Profile("Analyzing assemblies dependencies ...", () =>
            {
                var deps = analyzer.GetRecursiveDependencies(sources);

                return new SpecialGraphBuilder()
                    .CreateGraphOfReachables(
                        sources.Select(R.AssemblyName),
                        targets.Select(x => R.AssemblyName(x.Item1)),
                        deps.Select(r => (R.AssemblyName(r.Assembly), R.AssemblyName(r.Dependency))));

            });

            if (AssemblyReferencesOnly)
            {
                DocumentSerializer.Serialize(outputFile, assemblyGraphPresentation);
            }
            else
            {
                var callsPresentation = Profile("Analyze method calls ...", () =>
                    BuildCallTree(sources, targets.Select(x => x.Item2).ToList(), assemblyGraphPresentation));

                if (StrictCallsOnly)
                {
                    Profile("Analyze direct method call dependencies ...", () =>
                    {
                        var p = ReduceToDirectDependencies(targets.Select(x => x.Item2).ToList(), callsPresentation);
                        DocumentSerializer.Serialize(outputFile, p);
                        return 0;
                    });
                }
                else
                {
                    Profile("Analyze indirect type dependencies ...", () =>
                    {
                        var p = ReduceToIndirectDependencies(targets.Select(x => x.Item2).ToList(), callsPresentation);
                        DocumentSerializer.Serialize(outputFile, p);
                        return 0;
                    });
                }
            }
        }

        public void Execute(string configFile, bool assemblyReferencesOnly, bool strictCallsOnly, string outputFile)
        {
            var definition = new
            {
                NetFramework = false,
                BinFolder = "",
                Sources = new[] { "" },
                Targets = new[] { new
                    {
                        Assembly = "",
                        Type = "",
                        Method = ""
                    } },
                RelevantAssemblies = new[] { "" }
            };

            using (var reader = new StreamReader(configFile))
            {
                var config = JsonConvert.DeserializeAnonymousType(reader.ReadToEnd(), definition);

                var sources = ResolveAssemblies(config.BinFolder, config.Sources).ToList();
                var targets = config.Targets
                    .Select(t => new TargetDescriptor
                    {
                        Assembly = ResolveAssemblies(config.BinFolder, t.Assembly).Single(),
                        Type = t.Type,
                        Method = t.Method
                    })
                    .ToList();

                AssemblyReferencesOnly = assemblyReferencesOnly;
                StrictCallsOnly = strictCallsOnly;

                var loader = AssemblyLoaderFactory.Create(myLoggerFactory, config.NetFramework ? DotNetRuntime.Framework : DotNetRuntime.Core);
                Execute(loader, sources, targets, config.RelevantAssemblies, outputFile);
            }
        }

        private IEnumerable<string> ResolveAssemblies(string binFolder, string pattern)
        {
            var files = Directory.GetFiles(binFolder, pattern);
            if (files.Length == 0)
            {
                myLogger.LogWarning($"No assemblies found for pattern: {pattern}");
                return Enumerable.Empty<string>();
            }
            else
            {
                return files
                    .Select(f => Path.GetFullPath(f))
                    .ToList();
            }
        }

        private IEnumerable<string> ResolveAssemblies(string binFolder, IEnumerable<string> patterns)
        {
            return patterns.SelectMany(p => ResolveAssemblies(binFolder, p)).ToList();
        }
    }
}
