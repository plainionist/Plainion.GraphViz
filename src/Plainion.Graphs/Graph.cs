using System;
using System.Collections.Generic;
using System.Linq;

namespace Plainion.Graphs;

[Serializable]
public class Graph : IGraph
{
    private readonly Dictionary<string, Node> myNodes;
    private readonly Dictionary<string, Edge> myEdges;
    private readonly Dictionary<string, Cluster> myClusters;

    public Graph()
    {
        myNodes = [];
        myEdges = [];
        myClusters = [];
    }

    public IReadOnlyCollection<Node> Nodes => myNodes.Values;
    public IReadOnlyCollection<Edge> Edges => myEdges.Values;
    public IReadOnlyCollection<Cluster> Clusters => myClusters.Values;

    public bool TryAdd(Node node)
    {
        Contract.RequiresNotNull(node, "node");

        Contract.Invariant(!IsFrozen, "Graph is frozen and cannot be modified");

        if (myNodes.ContainsKey(node.Id))
        {
            return false;
        }

        myNodes.Add(node.Id, node);

        return true;
    }

    public void Add(Node node)
    {
        if (!TryAdd(node))
        {
            throw new ArgumentException("Node already exists: " + node.Id);
        }
    }

    public bool TryAdd(Edge edge)
    {
        Contract.RequiresNotNull(edge, "edge");

        Contract.Invariant(!IsFrozen, "Graph is frozen and cannot be modified");

        if (myEdges.ContainsKey(edge.Id))
        {
            return false;
        }

        myEdges.Add(edge.Id, edge);

        return true;
    }

    public void Add(Edge edge)
    {
        if (!TryAdd(edge))
        {
            throw new ArgumentException("Edge already exists: " + edge.Id);
        }
    }

    public bool TryAdd(Cluster cluster)
    {
        Contract.RequiresNotNull(cluster, "cluster");

        Contract.Invariant(!IsFrozen, "Graph is frozen and cannot be modified");

        if (myClusters.ContainsKey(cluster.Id))
        {
            return false;
        }

        myClusters.Add(cluster.Id, cluster);

        return true;
    }

    public void Add(Cluster cluster)
    {
        if (!TryAdd(cluster))
        {
            throw new ArgumentException("Cluster already exists: " + cluster.Id);
        }
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

    public Node GetNode(string nodeId)
    {
        var node = FindNode(nodeId);
        Contract.Requires(node != null, "Node not found: " + nodeId);
        return node;
    }

    public bool IsFrozen { get; private set; }

    public void Freeze()
    {
        IsFrozen = true;
    }

    /// <summary>
    /// Creates a deep copy of this graph with new Node, Edge, and Cluster instances.
    /// The cloned graph is frozen.
    /// </summary>
    public Graph DeepClone()
    {
        var clone = new Graph();

        var nodeMap = new Dictionary<string, Node>(myNodes.Count);
        foreach (var node in myNodes.Values)
        {
            var clonedNode = new Node(node.Id);
            nodeMap[node.Id] = clonedNode;
            clone.Add(clonedNode);
        }

        foreach (var edge in myEdges.Values)
        {
            var clonedEdge = new Edge(nodeMap[edge.Source.Id], nodeMap[edge.Target.Id], edge.Weight);
            clone.Add(clonedEdge);

            nodeMap[edge.Source.Id].Out.Add(clonedEdge);
            nodeMap[edge.Target.Id].In.Add(clonedEdge);
        }

        foreach (var cluster in myClusters.Values)
        {
            var clonedCluster = new Cluster(cluster.Id, cluster.Nodes.Select(n => nodeMap[n.Id]));
            clone.Add(clonedCluster);
        }

        clone.Freeze();
        return clone;
    }
}
