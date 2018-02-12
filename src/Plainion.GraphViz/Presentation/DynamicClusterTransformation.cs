using System.Collections.Generic;
using System.Linq;
using Plainion.GraphViz.Model;
using Plainion.Windows.Mvvm;

namespace Plainion.GraphViz.Presentation
{
    /// <summary>
    /// Manages dynamics on clusters like: new clusters, hide clusters, rename clusters, etc
    /// </summary>
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

            OnPropertyChanged(nameof(ClusterVisibility));
        }

        public void HideCluster(string clusterId)
        {
            myClusterVisibility[clusterId] = false;

            OnPropertyChanged(nameof(ClusterVisibility));
        }

        public void ResetClusterVisibility(string clusterId)
        {
            myClusterVisibility.Remove(clusterId);

            OnPropertyChanged(nameof(ClusterVisibility));
        }

        public void AddToCluster(string nodeId, string clusterId)
        {
            myNodeToClusterMapping[nodeId] = clusterId;
            OnPropertyChanged(nameof(NodeToClusterMapping));
        }

        public void AddToCluster(IReadOnlyCollection<string> nodeIds, string clusterId)
        {
            Contract.RequiresNotNullNotEmpty(nodeIds, "nodeIds");

            foreach (var nodeId in nodeIds)
            {
                myNodeToClusterMapping[nodeId] = clusterId;
            }

            OnPropertyChanged(nameof(NodeToClusterMapping));
        }

        public void RemoveFromClusters(params string[] nodeIds)
        {
            foreach (var nodeId in nodeIds)
            {
                myNodeToClusterMapping[nodeId] = null;
            }

            OnPropertyChanged(nameof(NodeToClusterMapping));
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

            var newClusterIds = myClusterVisibility
                .Where(e => e.Value == true)
                .Where(e => !graph.Clusters.Any(c => c.Id == e.Key))
                .Select(e => e.Key);
            foreach (var clusterId in newClusterIds)
            {
                var nodes = myNodeToClusterMapping
                    .Where(e => e.Value == clusterId)
                    .Select(e => graph.Nodes.First(n => n.Id == e.Key));

                var newCluster = new Cluster(clusterId, nodes);
                result.Add(newCluster);
            }

            return result;
        }
    }
}
