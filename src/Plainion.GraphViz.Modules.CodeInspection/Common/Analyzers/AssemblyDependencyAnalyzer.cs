using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Plainion.GraphViz.Algorithms;
using Plainion.GraphViz.Model;
using Plainion.GraphViz.Presentation;
using Plainion.Text;

namespace Plainion.GraphViz.Modules.CodeInspection.Common.Analyzers
{
    class AssemblyDependencyAnalyzer
    {
        private class AssemblyReference
        {
            public Assembly myAssembly;
            public Assembly myDependency;
        }

        private IEnumerable<Wildcard> myRelevantAssemblies;
        private AssemblyLoader myLoader;

        public AssemblyDependencyAnalyzer(IEnumerable<string> relevantAssemblies)
        {
            myRelevantAssemblies = relevantAssemblies.Select(p => new Wildcard(p)).ToList();
            myLoader = new AssemblyLoader();
        }

        /// <summary>
        /// Creates a graph of all source and target assemblies and all assemblies directly and indirectly referenced
        /// by source assemblies which cause an indirect dependency to target assemblies
        /// </summary>
        public GraphPresentation CreateAssemblyGraph(IEnumerable<Assembly> sources, IEnumerable<Assembly> targets)
        {
            var deps = sources.Aggregate(new List<AssemblyReference>(), AnalyzeAssemblies);

            return CreateAssemblyRelationshipGraph(sources.ToList(), targets.ToList(), deps.ToList());
        }

        private List<AssemblyReference> AnalyzeAssemblies(List<AssemblyReference> analyzedAssemblies, Assembly asm)
        {
            Console.WriteLine($"  {asm.FullName}");

            return Analyze(analyzedAssemblies, asm);
        }

        private List<AssemblyReference> Analyze(List<AssemblyReference> analyzed, Assembly asm)
        {
            var dependencies = asm.GetReferencedAssemblies()
                .Select(x => myLoader.LoadAssembly(x))
                .Where(x => x != null && FollowAssembly(x))
                .ToList();

            var analyzedAssemblies = analyzed
                .Select(x => x.myAssembly)
                .Distinct()
                .ToList();

            var initialAcc = analyzed.ToList();
            initialAcc.Add(new AssemblyReference { myAssembly = asm, myDependency = asm });

            var indirectDeps = dependencies
                .Where(x => analyzedAssemblies.Any(a => a.FullName != x.FullName))
                .Where(FollowAssembly)
                .Aggregate(initialAcc, Analyze);

            return dependencies
                .Select(x => new AssemblyReference { myAssembly = asm, myDependency = x })
                .Concat(indirectDeps)
                .ToList();
        }

        private bool FollowAssembly(Assembly asm)
        {
            return myRelevantAssemblies.Any(p => p.IsMatch(asm.GetName().Name));
        }

        // Show only nodes which can reach the target cluster
        private GraphPresentation ReduceGraph(Cluster targetCluster, RelaxedGraphBuilder builder)
        {
            var presentation = new GraphPresentation();
            presentation.Graph = builder.Graph;

            var algo = new AddRemoveTransitiveHull(presentation);
            algo.Show = true;
            algo.Reverse = true;
            presentation.AddMask(algo.Compute(targetCluster.Nodes));

            presentation.AddMask(new RemoveNodesWithoutEdges(presentation).Compute());

            return presentation;
        }

        private GraphPresentation CreateAssemblyRelationshipGraph(IList<Assembly> sources, IList<Assembly> targets, IEnumerable<AssemblyReference> assemblyReferences)
        {
            var builder = new RelaxedGraphBuilder();

            var edges = assemblyReferences
                .Select(r => (R.AssemblyName(r.myAssembly), R.AssemblyName(r.myDependency)))
                .Where(x => x.Item1 != x.Item2);

            foreach (var (f, t) in edges)
            {
                builder.TryAddEdge(f, t);
            }

            var sourceNodes = sources.Select(R.AssemblyName).ToList();
            var targetNodes = targets.Select(R.AssemblyName).ToList();

            foreach (var n in targetNodes)
            {
                builder.TryAddNode(n);
            }

            var targetCluster = builder.TryAddCluster("TARGET", targetNodes);
            builder.TryAddCluster("SOURCE", sourceNodes);

            return ReduceGraph(targetCluster, builder);
        }
    }
}
