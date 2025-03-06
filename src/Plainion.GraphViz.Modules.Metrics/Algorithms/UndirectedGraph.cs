using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace Plainion.Graphs.Undirected;

[DebuggerDisplay("{Id}")]
class Node : IGraphItem, IEquatable<Node>
{
    public Node(string id)
    {
        Contract.RequiresNotNullNotEmpty(id, nameof(id));

        Id = id;

        Neighbors = [];
    }

    public string Id { get; }

    public IList<Node> Neighbors { get; }

    public bool Equals(Node other) => other != null && Id == other.Id;
    public override bool Equals(object obj) => Equals(obj as Node);
    public override int GetHashCode() => Id.GetHashCode();
}

class RelaxedGraphBuilder
{
    private readonly Dictionary<string, Node> myNodes = [];

    public Node Start { get; private set; }

    public void TryAddEdge(string sourceNodeId, string targetNodeId)
    {
        var sourceNode = GetOrCreateNode(sourceNodeId);
        var targetNode = GetOrCreateNode(targetNodeId);

        sourceNode.Neighbors.Add(targetNode);
        targetNode.Neighbors.Add(sourceNode);
    }

    private Node GetOrCreateNode(string id)
    {
        if (!myNodes.TryGetValue(id, out var node))
        {
            node = new Node(id);
            myNodes.Add(id, node);

            Start ??= node;
        }

        return node;
    }

    public static Node Convert(IGraph graph)
    {
        var builder = new RelaxedGraphBuilder();
        foreach (var edge in graph.Edges)
        {
            builder.TryAddEdge(edge.Source.Id, edge.Target.Id);
        }
        return builder.Start;
    }
}

static class UndirectedGraph
{
    public static IEnumerable<(Node, Node)> Edges(this Node self) => self.WalkEdges().OrderBy(x => (x.Item1.Id, x.Item2.Id));

    private static IEnumerable<(Node, Node)> WalkEdges(this Node self)
    {
        if (self == null)
        {
            yield break;
        }

        var visited = new HashSet<string>();
        var coveredEdges = new HashSet<(Node, Node)>();
        var queue = new Queue<Node>();

        queue.Enqueue(self);
        visited.Add(self.Id);

        while (queue.Count > 0)
        {
            var node = queue.Dequeue();

            foreach (var neighbor in node.Neighbors)
            {
                var edge = node.Id.CompareTo(neighbor.Id) < 0
                    ? (node, neighbor)
                    : (neighbor, node);

                if (!coveredEdges.Contains(edge))
                {
                    yield return edge;
                    coveredEdges.Add(edge);
                }

                if (!visited.Contains(neighbor.Id))
                {
                    visited.Add(neighbor.Id);
                    queue.Enqueue(neighbor);
                }
            }
        }
    }
}
