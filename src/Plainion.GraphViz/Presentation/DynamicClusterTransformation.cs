using System.Collections.Generic;
using System.Linq;
using Microsoft.Practices.Prism.Mvvm;
using Plainion.GraphViz.Model;

namespace Plainion.GraphViz.Presentation
{
    public class DynamicClusterTransformation : BindableBase, IGraphTransformation
    {
        private readonly Dictionary<string, string> myNodeToClusterMapping;

        public DynamicClusterTransformation()
        {
            myNodeToClusterMapping = new Dictionary<string, string>();
        }

        public IReadOnlyDictionary<string, string> NodeToClusterMapping
        {
            get { return myNodeToClusterMapping; }
        }

        public void AddToCluster(string nodeId, string clusterId)
        {
            myNodeToClusterMapping[nodeId] = clusterId;
            OnPropertyChanged( () => NodeToClusterMapping );
        }

        public void AddToCluster(IEnumerable<string> nodeIds, string clusterId)
        {
            foreach (var nodeId in nodeIds)
            {
                myNodeToClusterMapping[nodeId] = clusterId;
            }
            OnPropertyChanged(() => NodeToClusterMapping);
        }
        
        public void RemoveFromClusters(string nodeId)
        {
            myNodeToClusterMapping[nodeId] = null;
            OnPropertyChanged( () => NodeToClusterMapping );
        }

        public IGraph Transform(IGraph graph)
        {
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
                var nodes = cluster.Nodes
                    .Where(n => !myNodeToClusterMapping.ContainsKey(n.Id) || myNodeToClusterMapping[n.Id] == cluster.Id)
                    .Concat(graph.Nodes.Where(n => myNodeToClusterMapping.ContainsKey(n.Id) && myNodeToClusterMapping[n.Id] == cluster.Id));

                var newCluster = new Cluster(cluster.Id, nodes);
                result.Add(newCluster);
            }

            return result;
        }
    }
}
