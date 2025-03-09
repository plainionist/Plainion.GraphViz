using System.Collections.Generic;

namespace Plainion.Graphs.Undirected;

public class RelaxedGraphBuilder
{
    private readonly Dictionary<string, Node> myNodes = [];

    public IReadOnlyCollection<Node> Graph => myNodes.Values;

    public void TryAddNode(string id)
    {
        GetOrCreateNode(id);
    }

    public void TryAddEdge(string sourceId, string targetId)
    {
        var source = GetOrCreateNode(sourceId);
        var target = GetOrCreateNode(targetId);

        source.Neighbors.Add(target);
        target.Neighbors.Add(source);
    }

    private Node GetOrCreateNode(string id)
    {
        if (!myNodes.TryGetValue(id, out var node))
        {
            node = new Node(id);
            myNodes.Add(id, node);
        }

        return node;
    }

    public static IReadOnlyCollection<Node> Convert(IGraph graph)
    {
        var builder = new RelaxedGraphBuilder();
        foreach (var edge in graph.Edges)
        {
            builder.TryAddEdge(edge.Source.Id, edge.Target.Id);
        }
        return builder.Graph;
    }
}
