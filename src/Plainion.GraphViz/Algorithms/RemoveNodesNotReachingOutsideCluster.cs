using System.Linq;
using Plainion.GraphViz.Model;
using Plainion.GraphViz.Presentation;

namespace Plainion.GraphViz.Algorithms
{
    /// <summary>
    /// Generates "hide mask" removing all visible nodes from the given cluster which are not reaching the outside of the cluster.
    /// </summary>
    public class RemoveNodesNotReachingOutsideCluster : AbstractAlgorithm
    {
        public RemoveNodesNotReachingOutsideCluster(IGraphPresentation presentation)
            :base(presentation)
        {
        }

        public INodeMask Compute(Cluster cluster)
        {
            var nodes = cluster.Nodes
                .Where(n => !n.Out.Any(Presentation.Picking.Pick))
                .ToList();

            var moreFound = true;
            while (moreFound)
            {
                var moreNodes = cluster.Nodes
                    .Except(nodes)
                    .Where(n => n.Out.All(e => !Presentation.Picking.Pick(e.Target) || nodes.Contains(e.Target)))
                    .ToList();

                moreFound = moreNodes.Any();
                nodes.AddRange(moreNodes);
            }

            var mask = new NodeMask();
            mask.IsShowMask = false;
            mask.Set(nodes);

            var caption = Presentation.GetPropertySetFor<Caption>().Get(cluster.Id);
            mask.Label = $"Nodes not reaching outside {caption.DisplayText}";

            return mask;
        }
    }
}
