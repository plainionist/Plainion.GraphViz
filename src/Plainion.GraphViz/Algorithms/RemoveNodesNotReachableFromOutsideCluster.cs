using System.Linq;
using Plainion.GraphViz.Model;
using Plainion.GraphViz.Presentation;

namespace Plainion.GraphViz.Algorithms
{
    /// <summary>
    /// Generates "hide mask" removing all visible nodes from the given cluster which are not reachable from outside the cluster.
    /// </summary>
    public class RemoveNodesNotReachableFromOutsideCluster : AbstractAlgorithm
    {
        public RemoveNodesNotReachableFromOutsideCluster(IGraphPresentation presentation)
            :base(presentation)
        {
        }

        public INodeMask Compute(Cluster cluster)
        {
            var nodes = cluster.Nodes
                .Where(n => !n.In.Any(Presentation.Picking.Pick))
                .ToList();

            var moreFound = true;
            while (moreFound)
            {
                var moreNodes = cluster.Nodes
                    .Except(nodes)
                    .Where(n => n.In.All(e => !Presentation.Picking.Pick(e.Source) || nodes.Contains(e.Source)))
                    .ToList();

                moreFound = moreNodes.Any();
                nodes.AddRange(moreNodes);
            }

            var mask = new NodeMask();
            mask.IsShowMask = false;
            mask.Set(nodes);

            var caption = Presentation.GetPropertySetFor<Caption>().Get(cluster.Id);
            mask.Label = $"Nodes not reachable from outside {caption.DisplayText}";

            return mask;
        }
    }
}
