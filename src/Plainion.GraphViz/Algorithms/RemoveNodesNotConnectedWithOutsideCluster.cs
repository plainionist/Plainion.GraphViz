using System.Linq;
using Plainion.GraphViz.Model;
using Plainion.GraphViz.Presentation;

namespace Plainion.GraphViz.Algorithms
{
    /// <summary>
    /// Generates "hide mask" removing all visible nodes from the given cluster which are not connected to any node outside the cluster
    /// </summary>
    public class RemoveNodesNotConnectedWithOutsideCluster : AbstractAlgorithm
    {
        public RemoveNodesNotConnectedWithOutsideCluster(IGraphPresentation presentation)
            :base(presentation)
        {
        }

        public INodeMask Compute(Cluster cluster)
        {
            var nodes = cluster.Nodes
                .Where(n => !n.In.Any(Presentation.Picking.Pick) && !n.Out.Any(Presentation.Picking.Pick))
                .ToList();

            var moreFound = true;
            while (moreFound)
            {
                var moreNodes = cluster.Nodes
                    .Except(nodes)
                    .Where(n => n.In.All(e => !Presentation.Picking.Pick(e.Source) || nodes.Contains(e.Source)))
                    .Where(n => n.Out.All(e => !Presentation.Picking.Pick(e.Target) || nodes.Contains(e.Target)))
                    .ToList();

                moreFound = moreNodes.Any();
                nodes.AddRange(moreNodes);
            }

            var mask = new NodeMask();
            mask.IsShowMask = false;
            mask.Set(nodes);

            var caption = Presentation.GetPropertySetFor<Caption>().Get(cluster.Id);
            mask.Label = $"Nodes not connected with outside {caption.DisplayText}";

            return mask;
        }
    }
}
