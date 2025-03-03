using System.Linq;
using Plainion.Graphs.Projections;

namespace Plainion.Graphs.Algorithms;

/// <summary>
/// Generates "hide mask" removing all visible clusters and their nodes.
/// </summary>
public class RemoveClusters : AbstractAlgorithm
{
    public RemoveClusters(IGraphProjections presentation)
        : base(presentation)
    {
    }

    public INodeMask Compute()
    {
        var graph = Projections.TransformedGraph;

        var clusterNodes = graph.Nodes
            // do not process hidden nodes
            .Where(Projections.Picking.Pick)
            .Where(node => !graph.Clusters.Any(c => c.Nodes.Any(n => n.Id == node.Id)));

        var mask = new NodeMask();
        mask.IsShowMask = false;
        mask.Set(clusterNodes);
        mask.Label = "Nodes of clusters";

        return mask;
    }
}
