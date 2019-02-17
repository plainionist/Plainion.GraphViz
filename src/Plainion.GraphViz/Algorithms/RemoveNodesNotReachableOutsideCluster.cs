using System.Linq;
using Plainion.GraphViz.Model;
using Plainion.GraphViz.Presentation;

namespace Plainion.GraphViz.Algorithms
{
    /// <summary>
    /// Generates "hide mask" removing all visible nodes from the given cluster which are not reachable from outside the cluster.
    /// </summary>
    public class RemoveNodesNotReachableOutsideCluster : AbstractAlgorithm
    {
        public RemoveNodesNotReachableOutsideCluster(IGraphPresentation presentation)
            :base(presentation)
        {
        }

        public INodeMask Compute(Cluster cluster)
        {
            var unreachables = cluster.Nodes
                .Where(n => !n.In.Any(e => Presentation.Picking.Pick(e)))
                .ToList();

            var moreUnreachablesFound = true;
            while (moreUnreachablesFound)
            {
                var moreUnreachables = cluster.Nodes
                    .Except(unreachables)
                    .Where(n => n.In.All(e => !Presentation.Picking.Pick(e) || unreachables.Contains(e.Source)))
                    .ToList();

                moreUnreachablesFound = moreUnreachables.Any();
                unreachables.AddRange(moreUnreachables);
            }

            var mask = new NodeMask();
            mask.IsShowMask = false;
            mask.Set(unreachables);

            var caption = Presentation.GetPropertySetFor<Caption>().Get(cluster.Id);
            mask.Label = $"Nodes not reachable from outside {caption.DisplayText}";

            return mask;
        }
    }
}
