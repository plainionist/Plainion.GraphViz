using System.Collections.Generic;
using System.Linq;
using Plainion.Graphs.Projections;

namespace Plainion.Graphs.Algorithms;

/// <summary>
/// Returns all nodes which are not part of the given cluster.
/// </summary>
public class GetNodesOutsideCluster : AbstractAlgorithm
{
    public GetNodesOutsideCluster(IGraphProjections projections)
        : base(projections)
    {
    }

    public IReadOnlyCollection<string> Compute(string clusterId)
    {
        var graph = Projections.TransformedGraph;

        return graph.Nodes
            .Where(node => Projections.Picking.Pick(node))
            .Where(node => !graph.Clusters.Any(c => c.Nodes.Any(n => n.Id == node.Id)))
            .Select(node => node.Id)
            .ToList();
    }
}
