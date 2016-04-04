using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Practices.Prism.Mvvm;
using Plainion.GraphViz.Model;

namespace Plainion.GraphViz.Presentation
{
    public class ClusterFoldingTransformation : BindableBase, IGraphTransformation
    {
        private readonly IGraphPresentation myPresentation;

        // key: cluster.Id, value: cluster-node-id
        private readonly Dictionary<string, string> myFoldedClusters;

        // we remember the most recent input graph so that we can figure out
        // later which nodes where in which cluster BEFORE folding
        private IGraph myGraph;

        public ClusterFoldingTransformation(IGraphPresentation presentation)
        {
            myPresentation = presentation;

            myFoldedClusters = new Dictionary<string, string>();
        }

        public IEnumerable<string> Clusters
        {
            get { return myFoldedClusters.Keys; }
        }

        public IEnumerable<Node> GetNodes(string clusterId)
        {
            var graph = myGraph ?? myPresentation.Graph;

            return graph.Clusters.Single(c => c.Id == clusterId).Nodes;
        }

        public void Add(string clusterId)
        {
            if (myFoldedClusters.ContainsKey(clusterId))
            {
                return;
            }

            var clusterNodeId = Guid.NewGuid().ToString();

            myFoldedClusters.Add(clusterId, clusterNodeId);

            // encode cluster id again in caption to ensure that cluster is rendered big enough to include cluster caption
            var captions = myPresentation.GetPropertySetFor<Caption>();
            captions.Add(new Caption(clusterNodeId, "[" + captions.Get(clusterId).DisplayText + "]"));

            OnPropertyChanged(() => Clusters);
        }

        public void Remove(string clusterId)
        {
            myFoldedClusters.Remove(clusterId);

            OnPropertyChanged(() => Clusters);
        }

        public void Toggle(string clusterId)
        {
            if (myFoldedClusters.ContainsKey(clusterId))
            {
                Remove(clusterId);
            }
            else
            {
                Add(clusterId);
            }
        }

        public IGraph Transform(IGraph graph)
        {
            myGraph = graph;

            if (myFoldedClusters.Count == 0)
            {
                return graph;
            }

            var builder = new RelaxedGraphBuilder();

            var clusteredNodes = new List<string>();

            // add unfolded clusters
            foreach (var cluster in graph.Clusters.Where(c => !myFoldedClusters.ContainsKey(c.Id)))
            {
                var nodes = cluster.Nodes
                    .Select(n => n.Id)
                    .ToList();
                builder.TryAddCluster(cluster.Id, nodes);

                clusteredNodes.AddRange(nodes);
            }

            // add folded clusters
            foreach (var entry in myFoldedClusters)
            {
                builder.TryAddNode(entry.Value);
                builder.TryAddCluster(entry.Key, new[] { entry.Value });

                var foldedCluster = graph.Clusters.Single(c => c.Id == entry.Key);
                var foldedNodes = foldedCluster.Nodes
                    .Select(n => n.Id)
                    .ToList();

                clusteredNodes.AddRange(foldedNodes);

                foreach (var edge in graph.Edges)
                {
                    var source = edge.Source.Id;
                    var target = edge.Target.Id;

                    if (foldedNodes.Contains(source))
                    {
                        source = entry.Value;
                    }

                    if (foldedNodes.Contains(target))
                    {
                        target = entry.Value;
                    }

                    // ignore self-edges
                    if (source != target)
                    {
                        builder.TryAddEdge(source, target);
                    }
                }
            }

            // add non-clustered nodes
            foreach (var node in graph.Nodes.Select(n => n.Id).Except(clusteredNodes))
            {
                builder.TryAddNode(node);
            }

            return builder.Graph;
        }
    }
}
