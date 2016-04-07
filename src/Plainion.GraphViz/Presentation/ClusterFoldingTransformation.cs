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

            var nodesToClusterMap = new Dictionary<string, string>();

            // add unfolded clusters
            foreach (var cluster in graph.Clusters.Where(c => !myFoldedClusters.ContainsKey(c.Id)))
            {
                var nodes = cluster.Nodes
                    .Select(n => n.Id)
                    .ToList();
                builder.TryAddCluster(cluster.Id, nodes);

                foreach (var n in nodes)
                {
                    nodesToClusterMap[n] = cluster.Id;
                }
            }

            // add folded clusters
            foreach (var entry in myFoldedClusters.ToList())
            {
                builder.TryAddNode(entry.Value);
                builder.TryAddCluster(entry.Key, new[] { entry.Value });

                var foldedCluster = graph.Clusters.SingleOrDefault(c => c.Id == entry.Key);
                if (foldedCluster == null)
                {
                    // this cluster was deleted
                    myFoldedClusters.Remove(entry.Key);
                    continue;
                }

                var foldedNodes = foldedCluster.Nodes
                    .Select(n => n.Id)
                    .ToList();

                foreach (var n in foldedNodes)
                {
                    nodesToClusterMap[n] = foldedCluster.Id;
                }
            }

            // add non-clustered nodes
            foreach (var node in graph.Nodes.Select(n => n.Id).Except(nodesToClusterMap.Keys))
            {
                builder.TryAddNode(node);
            }

            // add edges 
            foreach (var edge in graph.Edges)
            {
                var source = edge.Source.Id;
                var target = edge.Target.Id;

                string foldedClusterId;
                string foldedClusterNodeId;
                if (nodesToClusterMap.TryGetValue(source, out foldedClusterId) && myFoldedClusters.TryGetValue(foldedClusterId, out foldedClusterNodeId))
                {
                    source = foldedClusterNodeId;
                }

                if (nodesToClusterMap.TryGetValue(target, out foldedClusterId) && myFoldedClusters.TryGetValue(foldedClusterId, out foldedClusterNodeId))
                {
                    target = foldedClusterNodeId;
                }

                // ignore self-edges
                if (source != target)
                {
                    builder.TryAddEdge(source, target);
                }
            }

            return builder.Graph;
        }
    }
}
