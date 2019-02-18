using System;
using System.Collections.Generic;
using System.Linq;
using Plainion.GraphViz.Model;
using Plainion.GraphViz.Presentation;

namespace Plainion.GraphViz.Algorithms
{
    /// <summary>
    /// Generates "hide mask" removing all visible nodes from the given cluster without siblings
    /// outside the cluster according to SiblingsType.
    /// </summary>
    public class RemoveNodesWithoutSiblingsOutsideOutsideCluster : AbstractAlgorithm
    {
        public RemoveNodesWithoutSiblingsOutsideOutsideCluster(IGraphPresentation presentation)
            : base(presentation)
        {
        }

        public SiblingsType SiblingsType { get; set; }

        public INodeMask Compute(Cluster cluster)
        {
            // we always want to operate on the "real" nodes of the cluster - even if the cluster is currently folded.
            // By that the user may not see any effect in the graph when working with folded clusters but this allows
            // us to filter the graph before unfolding and so avoid huge graphs.
            var nodesOfCluster = Presentation.Graph.Clusters.Single(c => c.Id == cluster.Id).Nodes;

            var empty = new List<Node>();
            var nodes = nodesOfCluster
                .Where(n => FilterNodes(n, empty))
                .ToList();

            var moreFound = true;
            while (moreFound)
            {
                var moreNodes = nodesOfCluster
                    .Except(nodes)
                    .Where(n => FilterNodes(n, nodes))
                    .ToList();

                moreFound = moreNodes.Any();
                nodes.AddRange(moreNodes);
            }

            var mask = new NodeMask();
            mask.IsShowMask = false;
            mask.Set(nodes);

            var caption = Presentation.GetPropertySetFor<Caption>().Get(cluster.Id);
            if (SiblingsType == SiblingsType.Any)
            {
                mask.Label = $"Nodes without siblings outside {caption.DisplayText}";
            }
            else if (SiblingsType == SiblingsType.Sources)
            {
                mask.Label = $"Nodes without sources outside {caption.DisplayText}";
            }
            else if (SiblingsType == SiblingsType.Targets)
            {
                mask.Label = $"Nodes without targets outside {caption.DisplayText}";
            }

            return mask;
        }

        private bool FilterNodes(Node n, List<Node> nodes)
        {
            if (SiblingsType == SiblingsType.Any)
            {
                return n.In.All(e => !Presentation.Picking.Pick(e.Source) || nodes.Contains(e.Source))
                    && n.Out.All(e => !Presentation.Picking.Pick(e.Target) || nodes.Contains(e.Target));
            }
            else if (SiblingsType == SiblingsType.Sources)
            {
                return n.In.All(e => !Presentation.Picking.Pick(e.Source) || nodes.Contains(e.Source));
            }
            else if (SiblingsType == SiblingsType.Targets)
            {
                return n.Out.All(e => !Presentation.Picking.Pick(e.Target) || nodes.Contains(e.Target));
            }

            throw new NotSupportedException(SiblingsType.ToString());
        }
    }
}
