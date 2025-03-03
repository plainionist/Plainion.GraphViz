using System;
using Plainion.Graphs.Projections;

namespace Plainion.Graphs.Algorithms.Tests;

internal class GraphProjections : IGraphProjections
{
    public GraphProjections(IGraph graph)
    {
        Contract.RequiresNotNull(graph);

        Graph = graph;
        TransformedGraph = graph;
        ClusterFolding = new ClusterFolding(this);
    }

    public IGraph Graph { get; }

    public IGraph TransformedGraph { get; }

    public IGraphPicking Picking { get; } = new NullGraphPicking();

    public IClusterFolding ClusterFolding { get; }

    public string GetCaption(string id) => id;
}