using System;
using System.Collections.Generic;
using System.Linq;
using Plainion.Graphs;

namespace Plainion.GraphViz.Modules.Metrics.Tests;

public class SimpleGraph : IGraph
{
    public IReadOnlyCollection<Node> Nodes { get; }
    public IReadOnlyCollection<Edge> Edges { get; }
    public IReadOnlyCollection<Cluster> Clusters { get; } = Array.Empty<Cluster>();

    public SimpleGraph(List<Node> nodes, List<Edge> edges)
    {
        Nodes = nodes;
        Edges = edges;
        foreach (var edge in edges)
        {
            edge.Source.Out.Add(edge);
            edge.Target.In.Add(edge);
        }
    }

    public Node FindNode(string nodeId) => Nodes.FirstOrDefault(n => n.Id == nodeId);
}
