using System.Collections.Generic;
using System.Linq;
using Microsoft.Practices.Prism.Mvvm;
using Plainion.GraphViz.Model;

namespace Plainion.GraphViz.Presentation
{
    public class DynamicClusterTransformation : BindableBase, IGraphTransformation
    {
        // key: nodeId, value: clusterId
        private readonly Dictionary<string, string> myNodeToClusterMapping;

        // key: clusterId, value: true == should be added if not already available, false = should be removed from graph
        private readonly Dictionary<string, bool> myClusterVisibility;

        public DynamicClusterTransformation()
        {
            myNodeToClusterMapping = new Dictionary<string, string>();
            myClusterVisibility = new Dictionary<string, bool>();
        }

        public IReadOnlyDictionary<string, string> NodeToClusterMapping
        {
            get { return myNodeToClusterMapping; }
        }

        public IReadOnlyDictionary<string, bool> ClusterVisibility
        {
            get { return myClusterVisibility; }
        }

        public void AddCluster(string clusterId)
        {
            myClusterVisibility[clusterId] = true;

            OnPropertyChanged(() => ClusterVisibility);
        }

        public void HideCluster(string clusterId)
        {
            myClusterVisibility[clusterId] = false;

            OnPropertyChanged(() => ClusterVisibility);
        }

        public void ResetClusterVisibility(string clusterId)
        {
            myClusterVisibility.Remove(clusterId);

            OnPropertyChanged(() => ClusterVisibility);
        }

        public void AddToCluster(string nodeId, string clusterId)
        {
            myNodeToClusterMapping[nodeId] = clusterId;
            OnPropertyChanged(() => NodeToClusterMapping);
        }

        public void AddToCluster(IEnumerable<string> nodeIds, string clusterId)
        {
            Contract.RequiresNotNullNotEmpty(nodeIds, "nodeIds");

            foreach (var nodeId in nodeIds)
            {
                myNodeToClusterMapping[nodeId] = clusterId;
            }

            OnPropertyChanged(() => NodeToClusterMapping);
        }

        public void RemoveFromClusters(string nodeId)
        {
            myNodeToClusterMapping[nodeId] = null;
            OnPropertyChanged(() => NodeToClusterMapping);
        }

        public IGraph Transform(IGraph graph)
        {
            if (myNodeToClusterMapping.Count == 0 && myClusterVisibility.Count == 0)
            {
                return graph;
            }

            var result = new Graph();

            foreach (var node in graph.Nodes)
            {
                result.Add(node);
            }

            foreach (var edge in graph.Edges)
            {
                result.Add(edge);
            }

            foreach (var cluster in graph.Clusters)
            {
                bool visibility;
                if (myClusterVisibility.TryGetValue(cluster.Id, out visibility) && !visibility)
                {
                    continue;
                }

                var nodes = cluster.Nodes
                    .Where(n => !myNodeToClusterMapping.ContainsKey(n.Id) || myNodeToClusterMapping[n.Id] == cluster.Id)
                    .Concat(graph.Nodes.Where(n => myNodeToClusterMapping.ContainsKey(n.Id) && myNodeToClusterMapping[n.Id] == cluster.Id));

                var newCluster = new Cluster(cluster.Id, nodes);
                result.Add(newCluster);
            }

            var newClusters = myClusterVisibility
                .Where(e => e.Value == true)
                .Where(e => !graph.Clusters.Any(c => c.Id == e.Key));
            foreach (var entry in newClusters)
            {
                var newCluster = new Cluster(entry.Key, Enumerable.Empty<Node>());
                result.Add(newCluster);
            }

            return result;
        }
    }
}
