using System.Linq;
using Plainion.GraphViz.Presentation;

namespace Plainion.GraphViz.Algorithms
{
    /// <summary>
    /// Generates "hide mask" removing all visible clusters and their nodes.
    /// </summary>
    public class RemoveClusters : AbstractAlgorithm
    {
        public RemoveClusters(IGraphPresentation presentation)
            : base(presentation)
        {
        }

        public INodeMask Compute()
        {
            var graph = Presentation.GetModule<ITransformationModule>().Graph;
            var clusterNodes = graph.Nodes
                .Where(Presentation.Picking.Pick)
                .Where(node => !(graph.Clusters.Any(c => c.Nodes.Any(n => n.Id == node.Id))));

            var mask = new NodeMask();
            mask.IsShowMask = false;
            mask.Set(clusterNodes);
            mask.Label = "Nodes of clusters";

            return mask;
        }
    }
}
