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
                var nodesOfCluster = Presentation.TransformedGraph().Clusters.Single(c => c.Id == cluster.Id).Nodes
                    // do not process hidden nodes
                    .Where(Presentation.Picking.Pick)
                    .ToList();

                var mask = new NodeMask
                {
                    IsShowMask = false,
                    Label = GetMaskLabel(cluster)
                };

                mask.Set(FindMatchingNodes(nodesOfCluster));

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

        private string GetMaskLabel(Cluster cluster)
        {
            var caption = Presentation.GetPropertySetFor<Caption>().Get(cluster.Id);
            if (SiblingsType == SiblingsType.Any)
            {
                return $"Nodes not connected with outside {caption.DisplayText}";
            }
            else if (SiblingsType == SiblingsType.Sources)
            {
                return $"Nodes not reachable from outside {caption.DisplayText}";
            }
            else if (SiblingsType == SiblingsType.Targets)
            {
                return $"Nodes reaching outside {caption.DisplayText}";
            }

            throw new NotSupportedException(SiblingsType.ToString());
        }

        private IEnumerable<Node> FindMatchingNodes(IReadOnlyCollection<Node> nodesOfCluster)
        {
            var empty = new List<Node>();

            // start analysing direct siblings of the cluster nodes
            var matchingNodes = nodesOfCluster
                .Where(n => IsMatchingNode(n, empty))
                .ToList();

            // continue analysing siblings recursively
            var moreFound = true;
            while (moreFound)
            {
                var moreNodes = nodesOfCluster
                    .Except(matchingNodes)
                    .Where(n => IsMatchingNode(n, matchingNodes))
                    .ToList();

                moreFound = moreNodes.Any();
                matchingNodes.AddRange(moreNodes);
            }

            return matchingNodes;
        }

        private bool IsMatchingNode(Node n, IReadOnlyCollection<Node> nodes)
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
