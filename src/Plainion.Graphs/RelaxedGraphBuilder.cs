using System;
using System.Collections.Generic;
using System.Linq;

namespace Plainion.Graphs;

[Serializable]
public class RelaxedGraphBuilder
{
    private readonly Graph myGraph = new();

    public virtual IGraph Graph => myGraph;

    /// <summary>
    /// Freezes the graph so that it cannot be altered any longer.
    /// </summary>
    public void Freeze()
    {
        myGraph.Freeze();
    }

    public Node TryAddNode(string nodeId)
    {
        var node = new Node(nodeId);

        if (!myGraph.TryAdd(node))
        {
            return null;
        }

        return node;
    }

    public Edge TryAddEdge(string sourceNodeId, string targetNodeId, int? weight = null)
    {
        var sourceNode = GetOrCreateNode(sourceNodeId);
        var targetNode = GetOrCreateNode(targetNodeId);

        var edge = weight.HasValue
            ? new Edge(sourceNode, targetNode, weight.Value)
            : new Edge(sourceNode, targetNode);

        if (!myGraph.TryAdd(edge))
        {
            return null;
        }

        edge.Source.Out.Add(edge);
        edge.Target.In.Add(edge);

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

    public Cluster TryAddCluster(string clusterId, IEnumerable<string> nodeIds)
    {
        var cluster = new Cluster(clusterId, nodeIds.Select(GetOrCreateNode));

        if (!myGraph.TryAdd(cluster))
        {
            return null;
        }

        return cluster;
    }
}
