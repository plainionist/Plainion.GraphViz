using System.Collections.Generic;
using System.Linq;
using Plainion.GraphViz.Model;
using Plainion.GraphViz.Presentation;

namespace Plainion.GraphViz.Algorithms
{
    /// <summary>
    /// Generates a "hide mask" removing all visible nodes not having edges of the given type.
    /// </summary>
    public class RemoveNodesWithoutEdges : AbstractAlgorithm
    {
        public RemoveNodesWithoutEdges(IGraphPresentation presentation)
            : base(presentation)
        {
        }

        public INodeMask Compute()
        {
            var transformationModule = Presentation.GetModule<ITransformationModule>();
            return Compute(transformationModule.Graph.Nodes);
        }

        private INodeMask Compute(IEnumerable<Node> nodes)
        {
            var nodesToHide = nodes
                .Where(n => HideNode(n));

            var mask = new NodeMask();
            mask.IsShowMask = false;
            mask.Set(nodesToHide);
            mask.Label = "Nodes without edges";

            return mask;
        }

        public INodeMask Compute(Cluster cluster)
        {
            return Compute(cluster.Nodes.Where(n => Presentation.Picking.Pick(n)));
        }

        private bool HideNode(Node node)
        {
            var noSources = !node.In.Any(e => Presentation.Picking.Pick(e));
            var noTargets = !node.Out.Any(e => Presentation.Picking.Pick(e));

            return noSources && noTargets;
        }
    }
}
