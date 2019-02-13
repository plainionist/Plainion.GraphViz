using System;
using System.Collections.Generic;
using System.Linq;
using Plainion.GraphViz.Model;
using Plainion.Windows.Mvvm;

namespace Plainion.GraphViz.Presentation
{
    /// <summary>
    /// Manages folding of clusters
    /// </summary>
    public class ClusterFoldingTransformation : BindableBase, IGraphTransformation, IDisposable
    {
        private readonly IGraphPresentation myPresentation;
        private readonly HashSet<string> myFoldedClusters;
        private IModuleChangedObserver myNodeMaskModuleObserver;
        private bool myTransformationTriggered;

        // we remember the most recent input graph so that we can figure out
        // later which nodes where in which cluster BEFORE folding
        private IGraph myRawGraph;

        private IGraph myRecentTransformedGraph;

        public ClusterFoldingTransformation(IGraphPresentation presentation)
        {
            myPresentation = presentation;

            myNodeMaskModuleObserver = myPresentation.GetModule<INodeMaskModule>().CreateObserver();
            myNodeMaskModuleObserver.ModuleChanged += OnGraphVisibilityChanged;

            myFoldedClusters = new HashSet<string>();
        }

        private void OnGraphVisibilityChanged(object sender, EventArgs e)
        {
            if (myRecentTransformedGraph == null)
            {
                // we will soon run transformation anyhow
                return;
            }

            if (myTransformationTriggered)
            {
                // transformation already triggered
                return;
            }

            var nextTransformedGraph = BuildGraph(myRawGraph);

            if (myRecentTransformedGraph.Edges.Count() == nextTransformedGraph.Edges.Count()
                && myRecentTransformedGraph.Edges.Intersect(nextTransformedGraph.Edges, new EdgeComparer()).Count() == nextTransformedGraph.Edges.Count())
            {
                // we only need to re-transform the graph if edges would change due to node visibility
                return;
            }

            myTransformationTriggered = true;

            // trigger a re-transformation
            OnPropertyChanged(nameof(Clusters));
        }

        public void Dispose()
        {
            if (myNodeMaskModuleObserver != null)
            {
                myNodeMaskModuleObserver.ModuleChanged -= OnGraphVisibilityChanged;
                myNodeMaskModuleObserver.Dispose();
                myNodeMaskModuleObserver = null;
            }
        }

        public IEnumerable<string> Clusters
        {
            get { return myFoldedClusters; }
        }

        public string GetClusterNodeId(string clusterId)
        {
            return "[" + clusterId + "]";
        }

        public IEnumerable<Node> GetNodes(string clusterId)
        {
            var graph = myRawGraph ?? myPresentation.Graph;

            return graph.Clusters.Single(c => c.Id == clusterId).Nodes;
        }

        public void Add(string clusterId)
        {
            if (myFoldedClusters.Contains(clusterId))
            {
                return;
            }

            AddInternal(clusterId);

            OnPropertyChanged(nameof(Clusters));
        }

        private void AddInternal(string clusterId)
        {
            var clusterNodeId = GetClusterNodeId(clusterId);

            // encode cluster id again in caption to ensure that cluster is rendered big enough to include cluster caption
            var captions = myPresentation.GetPropertySetFor<Caption>();
            if (!captions.Contains(clusterNodeId))
            {
                captions.Add(new Caption(clusterNodeId, "[" + captions.Get(clusterId).DisplayText + "]"));
            }

            myFoldedClusters.Add(clusterId);
        }

        public void Add(IEnumerable<string> clusterIds)
        {
            var clustersToAdd = clusterIds
                .Except(myFoldedClusters)
                .ToList();

            if (clustersToAdd.Count == 0)
            {
                return;
            }

            foreach (var cluster in clustersToAdd)
            {
                AddInternal(cluster);
            }

            OnPropertyChanged(nameof(Clusters));
        }

        public void Remove(string clusterId)
        {
            var removed = myFoldedClusters.Remove(clusterId);

            if (removed)
            {
                OnPropertyChanged(nameof(Clusters));
            }
        }

        public void Remove(IEnumerable<string> clusterIds)
        {
            var clustersToRemove = clusterIds
                .Intersect(myFoldedClusters)
                .ToList();

            if (clustersToRemove.Count == 0)
            {
                return;
            }

            foreach (var cluster in clustersToRemove)
            {
                myFoldedClusters.Remove(cluster);
            }

            OnPropertyChanged(nameof(Clusters));
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
            try
            {
                myRawGraph = graph;

                if (myFoldedClusters.Count == 0)
                {
                    return graph;
                }

                myRecentTransformedGraph = BuildGraph(graph);
                return myRecentTransformedGraph;
            }
            finally
            {
                myTransformationTriggered = false;
            }
        }

        private IGraph BuildGraph(IGraph graph)
        {
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
            var clusterMap = new Dictionary<string, Cluster>();
            foreach (var cluster in graph.Clusters)
            {
                clusterMap.Add(cluster.Id, cluster);
            }

            foreach (var clusterId in myFoldedClusters.ToList())
            {
                var clusterNodeId = GetClusterNodeId(clusterId);

                builder.TryAddNode(clusterNodeId);
                builder.TryAddCluster(clusterId, new[] { clusterNodeId });

                Cluster foldedCluster = null;
                if (!clusterMap.TryGetValue(clusterId, out foldedCluster))
                {
                    // this cluster was deleted
                    myFoldedClusters.Remove(clusterId);
                    continue;
                }

                var foldedNodes = foldedCluster.Nodes
                    // we add all nodes here for performance reasons - visibility is handled on "edge level" below
                    //.Where(n => myPresentation.Picking.Pick(n))
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

            // add "visible" edges 
            // ("rebind" edges to cluster nodes in case original source/target is folded now)
            foreach (var edge in graph.Edges.Where(e => myPresentation.Picking.Pick(e)))
            {
                var source = edge.Source.Id;
                var target = edge.Target.Id;

                string foldedClusterId;
                if (nodesToClusterMap.TryGetValue(source, out foldedClusterId) && myFoldedClusters.Contains(foldedClusterId))
                {
                    source = GetClusterNodeId(foldedClusterId);
                }

                if (nodesToClusterMap.TryGetValue(target, out foldedClusterId) && myFoldedClusters.Contains(foldedClusterId))
                {
                    target = GetClusterNodeId(foldedClusterId);
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
