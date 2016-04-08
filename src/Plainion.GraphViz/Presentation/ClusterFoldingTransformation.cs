﻿using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Practices.Prism.Mvvm;
using Plainion.GraphViz.Model;

namespace Plainion.GraphViz.Presentation
{
    public class ClusterFoldingTransformation : BindableBase, IGraphTransformation
    {
        private readonly IGraphPresentation myPresentation;

        // always keep node id of once folded clusters because we would generate new IDs when refolding
        // but that would destroy masks with folded clusters included ...
        // key: cluster.Id, value: cluster-node-id
        private readonly Dictionary<string, string> myClusterToClusterNodeMapping;

        private readonly HashSet<string> myFoldedClusters;

        // we remember the most recent input graph so that we can figure out
        // later which nodes where in which cluster BEFORE folding
        private IGraph myGraph;

        public ClusterFoldingTransformation(IGraphPresentation presentation)
        {
            myPresentation = presentation;

            myClusterToClusterNodeMapping = new Dictionary<string, string>();
            myFoldedClusters = new HashSet<string>();
        }

        public IEnumerable<string> Clusters
        {
            get { return myFoldedClusters; }
        }

        public string GetClusterNodeId(string clusterId)
        {
            return myClusterToClusterNodeMapping[clusterId];
        }

        public IEnumerable<Node> GetNodes(string clusterId)
        {
            var graph = myGraph ?? myPresentation.Graph;

            return graph.Clusters.Single(c => c.Id == clusterId).Nodes;
        }

        public void Add(string clusterId)
        {
            if (myFoldedClusters.Contains(clusterId))
            {
                return;
            }

            string clusterNodeId;
            if (!myClusterToClusterNodeMapping.TryGetValue(clusterId, out clusterNodeId))
            {
                clusterNodeId = Guid.NewGuid().ToString();
                myClusterToClusterNodeMapping.Add(clusterId, clusterNodeId);

                // encode cluster id again in caption to ensure that cluster is rendered big enough to include cluster caption
                var captions = myPresentation.GetPropertySetFor<Caption>();
                captions.Add(new Caption(clusterNodeId, "[" + captions.Get(clusterId).DisplayText + "]"));
            }

            myFoldedClusters.Add(clusterId);

            OnPropertyChanged(() => Clusters);
        }

        public void Remove(string clusterId)
        {
            myFoldedClusters.Remove(clusterId);

            OnPropertyChanged(() => Clusters);
        }

        public void Toggle(string clusterId)
        {
            if (myFoldedClusters.Contains(clusterId))
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
            foreach (var cluster in graph.Clusters.Where(c => !myFoldedClusters.Contains(c.Id)))
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
            foreach (var clusterId in myFoldedClusters.ToList())
            {
                var clusterNodeId = myClusterToClusterNodeMapping[clusterId];

                builder.TryAddNode(clusterNodeId);
                builder.TryAddCluster(clusterId, new[] { clusterNodeId });

                var foldedCluster = graph.Clusters.SingleOrDefault(c => c.Id == clusterId);
                if (foldedCluster == null)
                {
                    // this cluster was deleted
                    myFoldedClusters.Remove(clusterId);
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
                if (nodesToClusterMap.TryGetValue(source, out foldedClusterId) && myFoldedClusters.Contains(foldedClusterId))
                {
                    source = myClusterToClusterNodeMapping[foldedClusterId];
                }

                if (nodesToClusterMap.TryGetValue(target, out foldedClusterId) && myFoldedClusters.Contains(foldedClusterId))
                {
                    target = myClusterToClusterNodeMapping[foldedClusterId];
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
