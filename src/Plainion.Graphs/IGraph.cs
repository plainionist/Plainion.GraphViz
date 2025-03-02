using System.Collections.Generic;

namespace Plainion.Graphs;

public interface IGraph
{
    IReadOnlyCollection<Node> Nodes { get; }
    IReadOnlyCollection<Edge> Edges { get; }
    IReadOnlyCollection<Cluster> Clusters { get; }

    Node FindNode(string nodeId);
}
