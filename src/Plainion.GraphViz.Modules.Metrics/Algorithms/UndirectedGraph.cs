using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace Plainion.Graphs.Undirected;

class Graph
{ 
    private readonly Dictionary<string, Node> myNodes;
    private readonly Dictionary<string, Edge> myEdges;

    public Graph()
    {
        myNodes = [];
        myEdges = [];
    }

    public IReadOnlyCollection<Node> Nodes => myNodes.Values;
    public IReadOnlyCollection<Edge> Edges => myEdges.Values;

    public bool TryAdd(Node node)
    {
        Contract.RequiresNotNull(node, "node");

        if (myNodes.ContainsKey(node.Id))
        {
            return false;
        }

        myNodes.Add(node.Id, node);

        return true;
    }

    public bool TryAdd(Edge edge)
    {
        Contract.RequiresNotNull(edge, "edge");

        if (myEdges.ContainsKey(edge.Id))
        {
            return false;
        }

        myEdges.Add(edge.Id, edge);

        return true;
    }

    public Node FindNode(string nodeId)
    {
        Node node;
        if (myNodes.TryGetValue(nodeId, out node))
        {
            return node;
        }

        return null;
    }
}

[DebuggerDisplay("{Id}")]
class Node : IGraphItem, IEquatable<Node>
{
    public Node(string id)
    {
        Contract.RequiresNotNullNotEmpty(id, nameof(id));

        Id = id;

        Edges = [];
    }

    public string Id { get; }

    public IList<Edge> Edges { get; }

    public bool Equals(Node other) => other != null && Id == other.Id;
    public override bool Equals(object obj) => Equals(obj as Node);
    public override int GetHashCode() => Id.GetHashCode();
}

[DebuggerDisplay("{Source.Id} - {Target.Id}")]
class Edge : IGraphItem, IEquatable<Edge>
{
    public Edge(Node source, Node target)
    {
        Contract.RequiresNotNull(source, nameof(source));
        Contract.RequiresNotNull(target, nameof(target));

        var sorted = source.Id.CompareTo(target.Id);
        Source = sorted < 0 ? source : target;
        Target = sorted < 0 ? target : source;

        Id = $"edge-{Source.Id}-{Target.Id}";
    }

    public string Id { get; }

    public Node Source { get; }
    public Node Target { get; }

    public bool Equals(Edge other) => other != null && Id == other.Id;
    public override bool Equals(object obj) => Equals(obj as Edge);
    public override int GetHashCode() => Id.GetHashCode();
}

class RelaxedGraphBuilder
{
    private readonly Graph myGraph = new();

    public Graph Graph => myGraph;

    public Node TryAddNode(string nodeId)
    {
        var node = new Node(nodeId);

        if (!myGraph.TryAdd(node))
        {
            return null;
        }

        return node;
    }

    public Edge TryAddEdge(string sourceNodeId, string targetNodeId)
    {
        var sourceNode = GetOrCreateNode(sourceNodeId);
        var targetNode = GetOrCreateNode(targetNodeId);

        var edge = new Edge(sourceNode, targetNode);

        if (!myGraph.TryAdd(edge))
        {
            return null;
        }

        edge.Source.Edges.Add(edge);
        edge.Target.Edges.Add(edge);

        return edge;
    }

    private Node GetOrCreateNode(string nodeId)
    {
        var node = Graph.FindNode(nodeId);
        if (node == null)
        {
            node = new Node(nodeId);
            myGraph.TryAdd(node);
        }

        return node;
    }

    public static Graph Convert(IGraph graph)
    {
        var builder = new RelaxedGraphBuilder();
        foreach (var node in graph.Nodes)
        {
            builder.TryAddNode(node.Id);
        }
        foreach (var edge in graph.Edges)
        {
            builder.TryAddEdge(edge.Source.Id, edge.Target.Id);
        }
        return builder.Graph;
    }
}
