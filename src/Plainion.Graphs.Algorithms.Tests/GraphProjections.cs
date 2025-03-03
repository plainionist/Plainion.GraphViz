using System;
using Plainion.Graphs.Projections;

namespace Plainion.Graphs.Algorithms.Tests;

internal class GraphProjections(IGraph graph) : IGraphProjections
{
    public IGraph Graph => graph;

    public IGraph TransformedGraph => graph;

    public IGraphPicking Picking { get; } = new NullGraphPicking();

    public IClusterFolding ClusterFolding => throw new NotImplementedException();

    public string GetCaption(string id) => id;
}