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
    public class RemoveNodesNotConnectedOutsideCluster : AbstractAlgorithm
    {
        public RemoveNodesNotConnectedOutsideCluster(IGraphPresentation presentation)
            : base(presentation)
        {
        }

        public SiblingsType SiblingsType { get; set; }

        public INodeMask Compute(Cluster cluster)
        {
            var folding = Presentation.ClusterFolding();
            var clusterIsFolded = folding.Clusters.Contains(cluster.Id);
            if (clusterIsFolded)
            {
                // the algorithm should work on folded clusters as well so we need to unfold the cluster temporarily 
                folding.Remove(cluster.Id);
            }

            try
            {
                // re-fetch the cluster in case it was folded
                var clusterNodes = Presentation.TransformedGraph().Clusters.Single(c => c.Id == cluster.Id).Nodes
                    // do not process hidden nodes
                    .Where(Presentation.Picking.Pick)
                    .ToList();

                var mask = new NodeMask();
                mask.IsShowMask = false;

                mask.Set(FindNodes(clusterNodes));

                var caption = Presentation.GetPropertySetFor<Caption>().Get(cluster.Id);
                if (SiblingsType == SiblingsType.Any)
                {
                    mask.Label = $"Nodes not connected with outside {caption.DisplayText}";
                }
                else if (SiblingsType == SiblingsType.Sources)
                {
                    mask.Label = $"Nodes not reachable from outside {caption.DisplayText}";
                }
                else if (SiblingsType == SiblingsType.Targets)
                {
                    mask.Label = $"Nodes reaching outside {caption.DisplayText}";
                }

                return mask;
            }
            finally
            {
                if (clusterIsFolded)
                {
                    folding.Add(cluster.Id);
                }
            }
        }

        private IEnumerable<Node> FindNodes(IReadOnlyCollection<Node> clusterNodes)
        {
            var empty = new List<Node>();
            var nodes = clusterNodes
                .Where(n => FilterNodes(n, empty))
                .ToList();

            var moreFound = true;
            while (moreFound)
            {
                var moreNodes = clusterNodes
                    .Except(nodes)
                    .Where(n => FilterNodes(n, nodes))
                    .ToList();

                moreFound = moreNodes.Any();
                nodes.AddRange(moreNodes);
            }

            return nodes;
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
