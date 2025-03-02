using System.Collections.Generic;
using System.Linq;
using Plainion.GraphViz.Presentation;

namespace Plainion.Graphs.Algorithms;

/// <summary>
/// Returns all nodes which are not part of the given cluster.
/// </summary>
public class GetNodesOutsideCluster : AbstractAlgorithm
{
    public GetNodesOutsideCluster(IGraphPresentation presentation)
        : base(presentation)
    {
    }

    public IReadOnlyCollection<string> Compute(string clusterId)
    {
        var transformationModule = Presentation.GetModule<ITransformationModule>();

        return transformationModule.Graph.Nodes
            .Where(node => Presentation.Picking.Pick(node))
            .Where(node => !transformationModule.Graph.Clusters.Any(c => c.Nodes.Any(n => n.Id == node.Id)))
            .Select(node => node.Id)
            .ToList();
    }
}
