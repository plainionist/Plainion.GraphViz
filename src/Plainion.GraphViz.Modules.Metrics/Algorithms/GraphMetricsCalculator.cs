using System.Collections.Generic;
using System.Linq;
using Plainion.Graphs;

namespace Plainion.GraphViz.Modules.Metrics.Algorithms;

static class GraphMetricsCalculator
{
    /// <summary>
    /// Ratio of actual edges to the maximum possible edges in the graph
    /// </summary>
    public static double ComputeGraphDensity(IGraph graph) =>
        graph.Edges.Count == 0 ? 0 : (double)graph.Edges.Count / (graph.Nodes.Count * (graph.Nodes.Count - 1));

    /// <summary>
    /// Length of the longest shortest path between any two nodes in the graph
    /// https://en.wikipedia.org/wiki/Diameter_(graph_theory)
    /// </summary>
    public static int ComputeDiameter(ShortestPaths shortestPaths) =>
        shortestPaths.Paths.Count == 0 ? 0 : shortestPaths.Paths.Max(path => path.Count);

    /// <summary>
    /// The average shortest path length between two nodes in the graph
    /// https://en.wikipedia.org/wiki/Average_path_length
    /// </summary>
    public static double ComputeAveragePathLength(IGraph graph, ShortestPaths shortestPaths)
    {
        if (graph.Nodes.Count <= 1)
        {
            return 0.0;
        }

        var totalLength = shortestPaths.Paths.Sum(path => path.Count);
        var maxPairs = graph.Nodes.Count * (graph.Nodes.Count - 1);

        return (double)totalLength / maxPairs;
    }

    /// <summary>
    /// Measures how often a node is a member of the shortest path between other nodes
    /// https://en.wikipedia.org/wiki/Betweenness_centrality
    /// </summary>
    public static IReadOnlyCollection<BetweennessCentrality> ComputeBetweennessCentrality(IGraph graph, ShortestPaths shortestPaths)
    {
        // (source, target) -> number of paths
        var pathCounts = new Dictionary<(Node, Node), int>();
        var nodePathCounts = new Dictionary<(Node, Node), Dictionary<Node, int>>(); // (s, t) -> (v -> count)
        var maxPairs = (graph.Nodes.Count - 1) * (graph.Nodes.Count - 2);

        // Count all shortest paths between 2 nodes
        // and how often each node is part of each path (ignoring start and end)
        foreach (var path in shortestPaths.Paths)
        {
            var pathId = (path.Start, path.End);
            pathCounts[pathId] = pathCounts.GetValueOrDefault(pathId) + 1;

            if (!nodePathCounts.ContainsKey(pathId))
            {
                nodePathCounts[pathId] = [];
            }

            // skip start and end, ignore target nodes as target of one edge is the source of the next
            // then count add 1 for each path the node is part of
            foreach (var node in path.Skip(1).Select(e => e.Source))
            {
                nodePathCounts[pathId][node] = nodePathCounts[pathId].GetValueOrDefault(node) + 1;
            }
        }

        // Compute betweenness
        var betweenness = graph.Nodes.ToDictionary(n => n, _ => 0.0);
        foreach (var entry in nodePathCounts)
        {
            foreach (var nodeCount in entry.Value)
            {
                betweenness[nodeCount.Key] += (double)nodeCount.Value / pathCounts[entry.Key];
            }
        }

        return betweenness
            .Select(x => new BetweennessCentrality
            {
                Node = x.Key,
                Absolute = x.Value,
                Normalized = maxPairs == 0 ? 0.0 : x.Value / maxPairs,
            })
            .ToList();
    }

    /// <summary>
    /// Measures how often an edge is a member of the shortest path between other nodes
    /// </summary>
    public static IReadOnlyDictionary<Edge, double> ComputeEdgeBetweenness(IGraph graph, ShortestPaths shortestPaths)
    {
        var betweenness = graph.Edges.ToDictionary(e => e, _ => 0.0);
        var maxPairs = graph.Nodes.Count * (graph.Nodes.Count - 1);

        if (maxPairs == 0)
        {
            // empty graph;
            return betweenness;
        }

        foreach (var path in shortestPaths.Paths)
        {
            // add 1 for each path the edge is part of
            foreach (var edge in path)
            {
                betweenness[edge] += 1.0;
            }
        }

        foreach (var edgeId in betweenness.Keys)
        {
            betweenness[edgeId] /= maxPairs;
        }

        return betweenness;
    }
}