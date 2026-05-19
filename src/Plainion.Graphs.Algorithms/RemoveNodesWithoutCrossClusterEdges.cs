using System.Collections.Generic;
using System.Linq;
using Plainion.Graphs.Projections;

namespace Plainion.Graphs.Algorithms;

/// <summary>
/// Generates a "hide mask" removing all visible nodes that do not have at least one direct edge
/// to a node in a different cluster. Nodes not belonging to any cluster are always hidden.
/// Folded clusters are temporarily unfolded for analysis.
/// </summary>
public class RemoveNodesWithoutCrossClusterEdges : AbstractAlgorithm
{
    public RemoveNodesWithoutCrossClusterEdges(IGraphProjections projections)
        : base(projections)
    {
    }

    public INodeMask Compute()
    {
        var folding = Projections.ClusterFolding;

        var previouslyFolded = folding.Clusters.ToList();
        foreach (var id in previouslyFolded)
        {
            folding.Remove(id);
        }

        try
        {
            var graph = Projections.TransformedGraph;

            var nodeToCluster = BuildNodeToClusterMap(graph);

            var nodesToHide = graph.Nodes
                .Where(Projections.Picking.Pick)
                .Where(n => LacksCrossClusterEdge(n, nodeToCluster));

            var mask = new NodeMask
            {
                IsShowMask = false,
                Label = "Nodes without cross-cluster edges"
            };
            mask.Set(nodesToHide);

            return mask;
        }
        finally
        {
            foreach (var id in previouslyFolded)
            {
                folding.Add(id);
            }
        }
    }

    private static Dictionary<Node, string> BuildNodeToClusterMap(IGraph graph) =>
        graph.Clusters
            .SelectMany(c => c.Nodes.Select(n => (Node: n, ClusterId: c.Id)))
            .ToDictionary(x => x.Node, x => x.ClusterId);

    private bool LacksCrossClusterEdge(Node node, Dictionary<Node, string> nodeToCluster)
    {
        if (!nodeToCluster.TryGetValue(node, out var clusterId))
            return true;

        return !node.In.Any(e => IsVisibleCrossClusterNeighbor(e.Source, clusterId, nodeToCluster))
            && !node.Out.Any(e => IsVisibleCrossClusterNeighbor(e.Target, clusterId, nodeToCluster));
    }

    private bool IsVisibleCrossClusterNeighbor(Node neighbor, string clusterId,
        Dictionary<Node, string> nodeToCluster) =>
        Projections.Picking.Pick(neighbor)
        && nodeToCluster.TryGetValue(neighbor, out var neighborCluster)
        && neighborCluster != clusterId;
}
