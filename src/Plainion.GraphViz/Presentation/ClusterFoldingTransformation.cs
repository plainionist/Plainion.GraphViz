using System;
using System.Collections.Generic;
using System.Linq;
using Plainion.Graphs;
using Plainion.Graphs.Projections;

namespace Plainion.GraphViz.Presentation
{
    /// <summary>
    /// Manages folding of clusters
    /// </summary>
    public class ClusterFoldingTransformation : ClusterFolding, IGraphTransformation, IDisposable
    {
        private readonly IGraphPresentation myPresentation;
        private IModuleChangedObserver myNodeMaskModuleObserver;
        private IModuleChangedJournal<Caption> myCaptionsJournal;

        // we remember the most recent input graph so that we can figure out
        // later which nodes where in which cluster BEFORE folding
        private IGraph myGraph;

        /// <summary>
        /// Represents an aggregated edge between clusters derived from node to node edges
        /// </summary>
        private class ComputedEdge
        {
            public ComputedEdge(string sourceId, string targetId)
            {
                SourceId = sourceId;
                TargetId = targetId;
            }

            public string SourceId;
            public string TargetId;
            public bool IsVisible;
            public List<Edge> Originals = new List<Edge>();

            public int Weight => Originals.Sum(x => x.Weight);

            public bool ShouldBeVisibile(IGraphPicking picking)
            {
                return Originals.Any(e => picking.Pick(e));
            }
        }

        // key: computed edge id, value: edges built up the computed edge
        private readonly Dictionary<string, ComputedEdge> myComputedEdges;

        public ClusterFoldingTransformation(IGraphPresentation presentation)
            :base(presentation)
        {
            myPresentation = presentation;

            myNodeMaskModuleObserver = myPresentation.GetModule<INodeMaskModule>().CreateObserver();
            myNodeMaskModuleObserver.ModuleChanged += OnGraphVisibilityChanged;

            myCaptionsJournal = myPresentation.GetPropertySetFor<Caption>().CreateJournal();
            myCaptionsJournal.ModuleChanged += OnCaptionChanged;

            myComputedEdges = [];
        }

        // if the caption of the cluster changes we need to align the
        // caption of the "cluster node" shown when the cluster is folded
        private void OnCaptionChanged(object sender, EventArgs e)
        {
            var captions = myPresentation.GetPropertySetFor<Caption>();

            // need a copy as we modify the captions module in the loop
            foreach (var entry in myCaptionsJournal.Entries.ToList())
            {
                var clusterNodeId = GetClusterNodeId(entry.OwnerId);

                var caption = captions.TryGet(clusterNodeId);
                // support obfuscation (removal of all captions)
                if (caption != null)
                {
                    caption.DisplayText = string.IsNullOrWhiteSpace(entry.DisplayText) ? " " : "[" + entry.DisplayText + "]";
                }
            }

            myCaptionsJournal.Clear();
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

            if (IsChangeNotified)
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

        public void Dispose()
        {
            if (myNodeMaskModuleObserver != null)
            {
                myNodeMaskModuleObserver.ModuleChanged -= OnGraphVisibilityChanged;
                myNodeMaskModuleObserver.Dispose();
                myNodeMaskModuleObserver = null;
            }

            myCaptionsJournal?.Dispose();
            myCaptionsJournal = null;
        }

        public override IReadOnlyCollection<Node> GetNodes(string clusterId)
        {
            var graph = myGraph ?? myPresentation.Graph;

            return graph.Clusters.Single(c => c.Id == clusterId).Nodes;
        }

        protected override void AddInternal(string clusterId)
        {
            var clusterNodeId = GetClusterNodeId(clusterId);

            // encode cluster id again in caption to ensure that cluster is rendered big enough to include cluster caption
            var captions = myPresentation.GetPropertySetFor<Caption>();
            if (!captions.Contains(clusterNodeId))
            {
                var displayText = captions.Get(clusterId).DisplayText;
                captions.Add(new Caption(clusterNodeId, string.IsNullOrWhiteSpace(displayText) ? " " : "[" + displayText + "]"));
            }

            base.AddInternal(clusterId);
        }

        public IGraph Transform(IGraph graph)
        {
            try
            {
                myGraph = graph;
                myComputedEdges.Clear();

                if (FoldedClusters.Count == 0)
                {
                    return graph;
                }

                return BuildGraph(graph);
            }
            finally
            {
                IsChangeNotified = false;
            }
        }

        private IGraph BuildGraph(IGraph graph)
        {
            var builder = new RelaxedGraphBuilder();

            var nodesToClusterMap = new Dictionary<string, string>();

            // add unfolded clusters
            foreach (var cluster in graph.Clusters.Where(c => !FoldedClusters.Contains(c.Id)))
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

            foreach (var clusterId in FoldedClusters.ToList())
            {
                var clusterNodeId = GetClusterNodeId(clusterId);

                builder.TryAddNode(clusterNodeId);
                builder.TryAddCluster(clusterId, new[] { clusterNodeId });

                Cluster cluster = null;
                if (!clusterMap.TryGetValue(clusterId, out cluster))
                {
                    // this cluster was deleted
                    FoldedClusters.Remove(clusterId);
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

            // "redirect" source/target in case source/target is inside folded cluster
            string GetNodeId(Node node)
            {
                return nodesToClusterMap.TryGetValue(node.Id, out var clusterId) && FoldedClusters.Contains(clusterId)
                    ? GetClusterNodeId(clusterId)
                    : node.Id;
            }

            // process edges
            foreach (var edge in graph.Edges)
            {
                var sourceId = GetNodeId(edge.Source);
                var targetId = GetNodeId(edge.Target);

                // Add all edges which are not folded (visibility of those is handled with masks)
                // Otherwise these edges are not "seen" when trying to extend the graph with "add" algorithms
                if (sourceId == edge.Source.Id && targetId == edge.Target.Id)
                {
                    // edge between two unfolded nodes
                    // -> add it
                    builder.TryAddEdge(sourceId, targetId, edge.Weight);

                    // nothing more to be done with this edge
                    continue;
                }

                // skip edges within a folded cluster
                if (sourceId == targetId)
                {
                    continue;
                }

                // ALWAYS remember based on what we computed the redirected edge.
                // Remember "decision" for when visibility of nodes/edges changes so that
                // we can check whether transformation has to be triggered again
                {
                    var redirectedEdgeId = Edge.CreateId(sourceId, targetId);
                    if (!myComputedEdges.TryGetValue(redirectedEdgeId, out var originalEdges))
                    {
                        originalEdges = new ComputedEdge(sourceId, targetId);
                        myComputedEdges.Add(redirectedEdgeId, originalEdges);
                    }

                    originalEdges.IsVisible |= myPresentation.Picking.Pick(edge);
                    originalEdges.Originals.Add(edge);
                }
            }

            // add folded/redirected edges
            foreach (var edge in myComputedEdges.Values)
            {
                // Only visible edges otherwise we would draw an edge which should not exist based on actual node visibility.
                // This makes the folding respect node visibility.
                if (edge.IsVisible)
                {
                    builder.TryAddEdge(edge.SourceId, edge.TargetId, edge.Weight);
                }
            }

            return builder.Graph;
        }
    }
}
