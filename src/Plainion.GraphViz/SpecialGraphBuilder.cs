using System.Collections.Generic;
using System.Linq;
using Plainion.GraphViz.Presentation;

namespace Plainion.Graphs.Algorithms;

public class SpecialGraphBuilder
{
    /// <summary>
    /// Creates a graph from given edges, only containing those nodes and edges which can be reached 
    /// from the source nodes, including the source nodes.
    /// </summary>
    public GraphPresentation CreateGraphOfReachables(IEnumerable<string> sourceNodes, IEnumerable<string> targetNodes, IEnumerable<(string, string)> references)
    {
        var (graph, targetCluster) = CreateGraph(sourceNodes, targetNodes, references);

        var presentation = new GraphPresentation
        {
            Graph = graph
        };

        RemoveUnreachableNodes(presentation, targetCluster);

        return presentation;
    }

    private static (IGraph, Cluster) CreateGraph(IEnumerable<string> sourceNodes, IEnumerable<string> targetNodes, IEnumerable<(string, string)> references)
    {
        var builder = new RelaxedGraphBuilder();
        foreach (var (f, t) in references.Where(x => x.Item1 != x.Item2))
        {
            builder.TryAddEdge(f, t);
        }

        foreach (var n in targetNodes)
        {
            builder.TryAddNode(n);
        }

        var targetCluster = builder.TryAddCluster("TARGET", targetNodes);
        builder.TryAddCluster("SOURCE", sourceNodes);

        return (builder.Graph, targetCluster);
    }

    private static void RemoveUnreachableNodes(GraphPresentation presentation, Cluster targetCluster)
    {
        var algo = new AddRemoveTransitiveHull(presentation, presentation)
        {
            Add = false,
            Reverse = true
        };

        var mask = algo.Compute(targetCluster.Nodes);
        mask.Invert(presentation.TransformedGraph,presentation.Picking);

        presentation.Masks().Push(mask);

        presentation.Masks().Push(new RemoveNodesWithoutSiblings(presentation).Compute());
    }
}
