using System;
using System.Linq;
using Plainion.Graphs.Projections;

namespace Plainion.Graphs.Algorithms.Tests;

public static class ModelExtensions
{
    public static Node GetNode(this IGraphProjections self, string id) =>
        self.Graph.Nodes.Single(x => x.Id == id);

    public static Cluster GetCluster(this IGraphProjections self, string id) =>
        self.Graph.Clusters.Single(x => x.Id == id);
}

/// <summary>
/// Test helper: projections with a custom per-node visibility predicate.
/// </summary>
public class CustomPickingProjections : IGraphProjections
{
    private readonly Func<string, bool> myIsVisible;

    public CustomPickingProjections(IGraph graph, Func<string, bool> isVisibleById)
    {
        Graph = graph;
        TransformedGraph = graph;
        myIsVisible = isVisibleById;
        ClusterFolding = new ClusterFolding(this);
        Picking = new CustomPicking(isVisibleById);
    }

    public IGraph Graph { get; }
    public IGraph TransformedGraph { get; }
    public IGraphPicking Picking { get; }
    public IClusterFolding ClusterFolding { get; }

    private class CustomPicking : IGraphPicking
    {
        private readonly Func<string, bool> myIsVisible;

        public CustomPicking(Func<string, bool> isVisible) => myIsVisible = isVisible;

        public bool Pick(Node node) => myIsVisible(node.Id);
        public bool Pick(Edge edge) => myIsVisible(edge.Source.Id) && myIsVisible(edge.Target.Id);
        public bool Pick(Cluster cluster) => true;
    }
}
