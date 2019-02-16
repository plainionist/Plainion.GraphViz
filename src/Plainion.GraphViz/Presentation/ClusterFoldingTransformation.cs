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
        private bool myChangeNotified;

        // we remember the most recent input graph so that we can figure out
        // later which nodes where in which cluster BEFORE folding
        private IGraph myGraph;

        // key: cluster id, value: nodes of this cluster which were visible during last rendering
        private readonly Dictionary<string, HashSet<string>> myRecentFoldedNodesVisibility;

        public ClusterFoldingTransformation(IGraphPresentation presentation)
        {
            myPresentation = presentation;

            myNodeMaskModuleObserver = myPresentation.GetModule<INodeMaskModule>().CreateObserver();
            myNodeMaskModuleObserver.ModuleChanged += OnGraphVisibilityChanged;

            myFoldedClusters = new HashSet<string>();
            myRecentFoldedNodesVisibility = new Dictionary<string, HashSet<string>>();
        }

        // If visibility of nodes/edges of a folded cluster changes that way that 
        // it would change the in/out edges of the folded cluster we need to 
        // trigger a transformation so that the "calculated" edges from/to the folded
        // cluster can be updated.
        private void OnGraphVisibilityChanged(object sender, EventArgs e)
        {
            if (!myFoldedClusters.SetEquals(myRecentFoldedNodesVisibility.Keys))
            {
                // folded clusters out of sync with last rendering state
                // -> transformation will anyhow be triggered soon outside
                return;
            }

            if (myChangeNotified)
            {
                // no need to notify again
                return;
            }

            IEnumerable<string> GetVisibleNodes(string clusterId) => GetNodes(clusterId).Where(n => myPresentation.Picking.Pick(n)).Select(n => n.Id);

            if (myRecentFoldedNodesVisibility.All(entry => entry.Value.SetEquals(GetVisibleNodes(entry.Key))))
            {
                // visibility of folded nodes did not change
                return;
            }

            myChangeNotified = true;

            // notify transformation has changed 
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
            var graph = myGraph ?? myPresentation.Graph;

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
                myGraph = graph;
                myRecentFoldedNodesVisibility.Clear();

                if (myFoldedClusters.Count == 0)
                {
                    return graph;
                }

                return BuildGraph(graph);
            }
            finally
            {
                myChangeNotified = false;
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

                var visibleNodes = new HashSet<string>();
                foreach (var n in foldedCluster.Nodes.Where(n => myPresentation.Picking.Pick(n)))
                {
                    nodesToClusterMap[n.Id] = foldedCluster.Id;
                    visibleNodes.Add(n.Id);
                }

                myRecentFoldedNodesVisibility.Add(clusterId, visibleNodes);
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

                string clusterId;
                if (nodesToClusterMap.TryGetValue(source, out clusterId) && myFoldedClusters.Contains(clusterId))
                {
                    source = GetClusterNodeId(clusterId);
                }

                if (nodesToClusterMap.TryGetValue(target, out clusterId) && myFoldedClusters.Contains(clusterId))
                {
                    target = GetClusterNodeId(clusterId);
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
