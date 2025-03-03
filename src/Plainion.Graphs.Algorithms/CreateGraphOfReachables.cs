using System.Collections.Generic;
using System.Linq;
using Plainion.Graphs.Projections;

namespace Plainion.Graphs.Algorithms;

public class CreateGraphOfReachablesResponse
{
    public required IGraph Graph { get; init; }
    public required Stack<INodeMask> Masks { get; init; }
}

/// <summary>
/// Creates a graph from given edges, only containing those nodes and edges which can be reached 
/// from the source nodes, including the source nodes.
/// </summary>
public class CreateGraphOfReachables
{
    public CreateGraphOfReachablesResponse Compute(IEnumerable<string> sourceNodes, IEnumerable<string> targetNodes, IEnumerable<(string, string)> references)
    {
        var (graph, targetCluster) = CreateGraph(sourceNodes, targetNodes, references);

        var masks = RemoveUnreachableNodes(graph, targetCluster);

        return new CreateGraphOfReachablesResponse
        {
            Graph = graph,
            Masks = masks
        };
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

    private static Stack<INodeMask> RemoveUnreachableNodes(IGraph graph, Cluster targetCluster)
    {
        var projections = new NullGraphProjections(graph);

        var algo = new AddRemoveTransitiveHull(projections, new NullCaptionProvider())
        {
            Add = false,
            Reverse = true
        };

        var mask = algo.Compute(targetCluster.Nodes);
        mask.Invert(graph, new NullGraphPicking());

        var masks = new Stack<INodeMask>();
        masks.Push(mask);

        masks.Push(new RemoveNodesWithoutSiblings(projections).Compute());

        return masks;
    }
}
