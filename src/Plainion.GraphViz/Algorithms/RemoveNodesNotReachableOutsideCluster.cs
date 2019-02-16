using System.Linq;
using Plainion.GraphViz.Model;
using Plainion.GraphViz.Presentation;

namespace Plainion.GraphViz.Algorithms
{
    public class RemoveNodesNotReachableOutsideCluster
    {
        private readonly IGraphPresentation myPresentation;

        public RemoveNodesNotReachableOutsideCluster(IGraphPresentation presentation)
        { 
            Contract.RequiresNotNull(presentation, "presentation");

            myPresentation = presentation;
        }

        public void Execute(Cluster cluster)
        {
            var unreachables = cluster.Nodes
                .Where(n => !n.In.Any(e => myPresentation.Picking.Pick(e)))
                .ToList();

            var moreUnreachablesFound = true;
            while (moreUnreachablesFound)
            {
                var moreUnreachables = cluster.Nodes
                    .Except(unreachables)
                    .Where(n => n.In.All(e => !myPresentation.Picking.Pick(e) || unreachables.Contains(e.Source)))
                    .ToList();

                moreUnreachablesFound = moreUnreachables.Any();
                unreachables.AddRange(moreUnreachables);
            }

            var mask = new NodeMask();
            mask.IsShowMask = false;
            mask.Set(unreachables);
            var caption = myPresentation.GetPropertySetFor<Caption>().Get(cluster.Id);
            mask.Label = $"Nodes not reachable from outside {caption.DisplayText}";

            var module = myPresentation.GetModule<INodeMaskModule>();
            module.Push(mask);
        }
    }
}
