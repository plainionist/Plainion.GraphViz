using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using Newtonsoft.Json;
using Plainion.GraphViz.Algorithms;
using Plainion.GraphViz.Model;
using Plainion.GraphViz.Modules.CodeInspection.Core;
using Plainion.GraphViz.Presentation;

namespace Plainion.GraphViz.Modules.CodeInspection.Batch
{
    public class TargetDescriptor
    {
        public string Assembly { get; set; }
        public string Type { get; set; }
        public string Method { get; set; }
    }

    public class CallTreeAnalyzer
    {
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

        private MonoLoader myLoader;
        private static Regex myTypeParameterPattern = new Regex(@"\[\[.*\]\]");

        public CallTreeAnalyzer()
        {
            myLoader = new MonoLoader();

        }

        public bool AssemblyReferencesOnly { get; set; }
        public bool StrictCallsOnly { get; set; }

        private MethodDesc CreateMethodDesc(Type declaringType, string name)
        {
            return new MethodDesc
            {
                myDeclaringType = declaringType,
                myName = name
            };
        }

        private IEnumerable<MethodCall> GetCalledMethods(Type t)
        {
            Console.Write(".");

            var inspector = new Inspector(myLoader, t);
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

        private Assembly GetAssembly(string name)
        {
            return AppDomain.CurrentDomain.ReflectionOnlyGetAssemblies()
                .Single(x => R.AssemblyName(x) == name);
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

        private MethodDesc GetMethod(Assembly assembly, string typeName, string methodName)
        {
            var declaringType = assembly.GetTypes().Single(t => t.FullName == typeName);

            var method = GetAllMethods(declaringType)
                .Where(name => name == methodName)
                .Distinct()
                .FirstOrDefault();

            if (method == null)
            {
                throw new InvalidOperationException($"Method not found {typeName}.{methodName} in {assembly.FullName}");
            }

            return CreateMethodDesc(declaringType, methodName);
        }

        private List<MethodCall> Analyze(IEnumerable<Node> relevantNodes, Func<MethodCall, IEnumerable<MethodCall>> replaceInterfacesWithImplementation, List<Type> analyzed, List<Type> callers)
        {
            var calls = callers.AsParallel()
                .SelectMany(t =>
                    GetCalledMethods(t)
                        .Where(r => CoveredByAssemblyGraph(relevantNodes, r.To.DeclaringType))
                        .SelectMany(replaceInterfacesWithImplementation))
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
                var children = Analyze(relevantNodes, replaceInterfacesWithImplementation, callers.Concat(analyzed).Distinct().ToList(), unknownTypes);
                return calls.Concat(children).ToList();
            }
        }

        // traces from the given source nodes all calls within the assembly graph
        private List<MethodCall> TraceCalles(IEnumerable<Node> relevantNodes, Func<MethodCall, IEnumerable<MethodCall>> replaceInterfacesWithImplementation, List<MethodDesc> targets, IEnumerable<Assembly> sources)
        {
            var targetTypes = targets
                .Select(m => m.myDeclaringType)
                .Distinct()
                .ToList();

            return Analyze(relevantNodes, replaceInterfacesWithImplementation, targetTypes,
                sources.SelectMany(x => x.GetTypes()).ToList())
                .Distinct()
                .ToList();
        }

        private static string GetInterfaceId(Type iface)
        {
            if (iface.AssemblyQualifiedName == null)
            {
                return null;
            }
            else if (iface.AssemblyQualifiedName.StartsWith("System."))
            {
                return null;
            }
            else if (iface.AssemblyQualifiedName.Contains("[["))
            {
                return myTypeParameterPattern.Replace(iface.AssemblyQualifiedName, "");
            }
            else
            {
                return iface.AssemblyQualifiedName;
            }
        }

        private GraphPresentation BuildCallTree(List<Assembly> sources, List<MethodDesc> targets, GraphPresentation assemblyGraphPresentation)
        {
            var relevantNodes = assemblyGraphPresentation.Graph.Nodes
                .Where(n => assemblyGraphPresentation.Picking.Pick(n))
                .ToList();

            var interfaceImplMap = relevantNodes
                .SelectMany(n => GetAssembly(n.Id).GetTypes())
                .SelectMany(t => t.GetInterfaces().Select(GetInterfaceId).Where(iface => iface != null).Select(iface => (iface, t)))
                .ToList();

            var implCountThreshold = 10;
            var tooGenericInterfaces = interfaceImplMap
                .GroupBy(x => x.Item1)
                .Where(x => implCountThreshold < x.Count())
                .Select(x => x.Key)
                .ToList();

            interfaceImplMap = interfaceImplMap.Where(x => !tooGenericInterfaces.Contains(x.Item1)).ToList();

            Func<MethodCall, IEnumerable<MethodCall>> replaceInterfacesWithImplementation = call =>
            {
                if (call.To.DeclaringType.IsInterface && (!targets.Any(t => t.myDeclaringType == call.To.DeclaringType)))
                {
                    var impls = interfaceImplMap.Where(x => x.Item1 == call.To.DeclaringType.AssemblyQualifiedName).ToList();
                    if (!impls.Any())
                    {
                        Shell.Warn($"Implementation not found for: {call.To.DeclaringType.FullName}");
                        return new[] { call };
                    }
                    else
                    {
                        Console.Write("#");
                        return impls.Select(x => new MethodCall(call.From, new Method(x.Item2, call.To.Name)));
                    }
                }
                else
                {
                    return new[] { call };
                }
            };

            var calls = TraceCalles(relevantNodes, replaceInterfacesWithImplementation, targets, sources);

            Console.WriteLine();
            Console.WriteLine("NOT analyzed assemblies:");

            foreach (var x in myLoader.SkippedAssemblies)
            {
                Shell.Warn($"    {x}");
            }

            return Shell.Profile("Generating call graph ...", () =>
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
                presentation.GetModule<INodeMaskModule>().AutoHideAllNodesForShowMasks = true;

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

            var masks = presentation.GetModule<INodeMaskModule>();
            masks.AutoHideAllNodesForShowMasks = true;

            // find all nodes from which the targets can be reached
            var algo = new TransitiveHull(presentation);
            algo.Show = true;
            algo.Reverse = true;

            var targetIds = targets.Select(target => MethodId(target.myDeclaringType, target.myName)).ToList();
            algo.Execute(presentation.Graph.Nodes.AsParallel()
                .Where(n => targetIds.Contains(n.Id))
                .ToList());

            return presentation;
        }

        // including type dependencies 
        private GraphPresentation ReduceToIndirectDependencies(List<MethodDesc> targets, GraphPresentation inputPresentation)
        {
            var presentation = new GraphPresentation();
            presentation.Graph = inputPresentation.Graph;

            CopyCaptions(presentation, inputPresentation);

            var masks = presentation.GetModule<INodeMaskModule>();
            masks.AutoHideAllNodesForShowMasks = true;

            var transformations = presentation.GetModule<ITransformationModule>();

            // 1. find clusters containing targets
            var targetIds = targets.Select(target => MethodId(target.myDeclaringType, target.myName)).ToList();
            var targetClusters = targetIds
                .Select(targetId => transformations.Graph.Clusters
                    .AsParallel()
                    .Single(c => c.Nodes.Any(n => n.Id == targetId)))
                .ToList();

            // 2. fold all clusters to "see" type dependencies only
            new ChangeClusterFolding(presentation).FoldUnfoldAllClusters();

            // 3. unfold clusters of targets again to only "see" dependencies to target nodes
            foreach (var c in targetClusters)
            {
                new ChangeClusterFolding(presentation).Execute(t => t.Toggle(c.Id));
            }

            // 4. find all nodes from which the targets can be reached
            var algo = new TransitiveHull(presentation);
            algo.Show = true;
            algo.Reverse = true;

            algo.Execute(transformations.Graph.Nodes.AsParallel()
                .Where(n => targetIds.Contains(n.Id))
                .ToList());

            return presentation;
        }

        public void Execute(IEnumerable<string> sourceAssemblies, IEnumerable<TargetDescriptor> targetDescriptors, IEnumerable<string> relevantAssemblies, string outputFile)
        {
            Console.WriteLine("Source assemblies:");
            foreach (var asm in sourceAssemblies)
            {
                Console.WriteLine($"  {asm}");
            }

            Console.WriteLine("Loading source assemblies ...");
            var sources = sourceAssemblies
                .Select(asm => R.LoadAssemblyFrom(asm))
                .Where(x => x != null)
                .ToList();

            var targets = Shell.Profile("Loading target methods ...", () =>
                targetDescriptors
                    .Select(target =>
                    {
                        var asm = R.LoadAssemblyFrom(target.Assembly);
                        return (asm, GetMethod(asm, target.Type, target.Method));
                    })
                    .ToList());

            var analyzer = new AssemblyDependencyAnalyzer(relevantAssemblies);
            var assemblyGraphPresentation = Shell.Profile("Analyzing assemblies dependencies ...", () =>
                analyzer.CreateAssemblyGraph(sources, targets.Select(x => x.Item1)));

            if (AssemblyReferencesOnly)
            {
                Graph.Serialize(outputFile, assemblyGraphPresentation);
            }
            else
            {
                var callsPresentation = Shell.Profile("Analyze method calls ...", () =>
                    BuildCallTree(sources, targets.Select(x => x.Item2).ToList(), assemblyGraphPresentation));

                if (StrictCallsOnly)
                {
                    Shell.Profile("Analyze direct method call dependencies ...", () =>
                    {
                        var p = ReduceToDirectDependencies(targets.Select(x => x.Item2).ToList(), callsPresentation);
                        Graph.Serialize(outputFile, p);
                        return 0;
                    });

                    Console.WriteLine(" Note:");
                    Console.WriteLine(" The 'strict' graph only considers direct call paths from one method to another.");
                    Console.WriteLine(" Dependencies between classes caused indirectly will later be removed, e.g.");
                    Console.WriteLine(" A.DoIt() -> B.DoIt() and B.DoItDifferently() -> C.Done()");
                    Console.WriteLine(" In this case the indirect dependency between A and C will be removed again.");
                    Console.WriteLine(" Due to the way how this removal is done the edge B.DoItDifferently() -> C.Done()");
                    Console.WriteLine(" will remain in the graph, which might be unexpected");
                    Console.WriteLine("");
                }
                else
                {
                    Shell.Profile("Analyze indirect type dependencies ...", () =>
                    {
                        var p = ReduceToIndirectDependencies(targets.Select(x => x.Item2).ToList(), callsPresentation);
                        Graph.Serialize(outputFile, p);
                        return 0;
                    });

                    Console.WriteLine(" Note:");
                    Console.WriteLine(" The 'relaxed' graph INCLUDES the indirect dependencies mentioned above.");
                    Console.WriteLine("");
                }
            }
        }

        public void Execute(string configFile, bool assemblyReferencesOnly, bool strictCallsOnly, string outputFile)
        {
            var definition = new
            {
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

                var sources = Shell.ResolveAssemblies(config.BinFolder, config.Sources).ToList();
                var targets = config.Targets
                    .Select(t => new TargetDescriptor
                    {
                        Assembly = Shell.ResolveAssemblies(config.BinFolder, t.Assembly).Single(),
                        Type = t.Type,
                        Method = t.Method
                    })
                    .ToList();

                AssemblyReferencesOnly = assemblyReferencesOnly;
                StrictCallsOnly = strictCallsOnly;

                Execute(sources, targets, config.RelevantAssemblies, outputFile);
            }
        }
    }
}
