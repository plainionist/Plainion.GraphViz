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

        private class ComputedEdge
        {
            public bool IsVisible;
            public List<Edge> Originals = new List<Edge>();

            public bool ShouldBeVisibile(IGraphPicking picking)
            {
                return Originals.Any(e => picking.Pick(e));
            }
        }

        // key: computed edge id, value: edges built up the computed edge
        private readonly Dictionary<string, ComputedEdge> myComputedEdges;

        public ClusterFoldingTransformation(IGraphPresentation presentation)
        {
            myPresentation = presentation;

            myNodeMaskModuleObserver = myPresentation.GetModule<INodeMaskModule>().CreateObserver();
            myNodeMaskModuleObserver.ModuleChanged += OnGraphVisibilityChanged;

            myFoldedClusters = new HashSet<string>();
            myComputedEdges = new Dictionary<string, ComputedEdge>();
        }

        // If visibility of nodes/edges of a folded cluster changes that way that 
        // it would change the in/out edges of the folded cluster we need to 
        // trigger a transformation so that the "calculated" edges from/to the folded
        // cluster can be updated.
        private void OnGraphVisibilityChanged(object sender, EventArgs eventArgs)
        {
            if (myGraph == null)
            {
                // no transformation happened so far
                // -> nothing to notify
                return;
            }

            if (myChangeNotified)
            {
                // no need to notify again
                return;
            }

            if (myComputedEdges.All(x => x.Value.IsVisible == x.Value.ShouldBeVisibile(myPresentation.Picking)))
            {
                // visibility of none of the computed edges would change
                return;
            }

            NotifyTransformationHasChanged();
        }

        private void NotifyTransformationHasChanged()
        {
            myChangeNotified = true;
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

            NotifyTransformationHasChanged();
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

            NotifyTransformationHasChanged();
        }

        public void Remove(string clusterId)
        {
            var removed = myFoldedClusters.Remove(clusterId);

            if (removed)
            {
                NotifyTransformationHasChanged();
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

            NotifyTransformationHasChanged();
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
                myComputedEdges.Clear();

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

                Cluster cluster = null;
                if (!clusterMap.TryGetValue(clusterId, out cluster))
                {
                    // this cluster was deleted
                    myFoldedClusters.Remove(clusterId);
                    continue;
                }

                // we can safely handle all nodes here as visibility is handled below on "edge-level"
                foreach (var n in cluster.Nodes)
                {
                    nodesToClusterMap[n.Id] = cluster.Id;
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
                var redirectedEdge = edge;

                // "redirect" source/target in case source/target is inside folded cluster

                if (nodesToClusterMap.TryGetValue(edge.Source.Id, out var clusterId) && myFoldedClusters.Contains(clusterId))
                {
                    redirectedEdge = new Edge(builder.Graph.FindNode(GetClusterNodeId(clusterId)), redirectedEdge.Target);
                }

                if (nodesToClusterMap.TryGetValue(edge.Target.Id, out clusterId) && myFoldedClusters.Contains(clusterId))
                {
                    redirectedEdge = new Edge(redirectedEdge.Source, builder.Graph.FindNode(GetClusterNodeId(clusterId)));
                }

                // ignore self-edges
                if (redirectedEdge.Source.Id == redirectedEdge.Target.Id)
                {
                    continue;
                }

                var isEdgeVisible = myPresentation.Picking.Pick(edge);
                if (isEdgeVisible)
                {
                    // only add visible nodes to the graph
                    builder.TryAddEdge(redirectedEdge.Source.Id, redirectedEdge.Target.Id);
                }

                if (redirectedEdge != edge)
                {
                    if (!myComputedEdges.TryGetValue(redirectedEdge.Id, out var originalEdges))
                    {
                        originalEdges = new ComputedEdge();
                        myComputedEdges.Add(redirectedEdge.Id, originalEdges);
                    }

                    originalEdges.IsVisible |= isEdgeVisible;
                    originalEdges.Originals.Add(edge);
                }
            }

            return builder.Graph;
        }
    }
}
