using System;

namespace Plainion.Graphs.Projections;

public class NullGraphProjections : IGraphProjections
{
    public NullGraphProjections(IGraph graph)
    {
        Contract.RequiresNotNull(graph);

        Graph = graph;
        TransformedGraph = graph;

        Picking = new NullGraphPicking();
        ClusterFolding = new ClusterFolding(this);
    }

    public IGraph Graph { get; }

    public IGraph TransformedGraph { get; }

    public IGraphPicking Picking { get; }

    public IClusterFolding ClusterFolding { get; }
}