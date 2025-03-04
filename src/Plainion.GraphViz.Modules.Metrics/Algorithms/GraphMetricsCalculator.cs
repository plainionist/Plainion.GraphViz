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
    public static IReadOnlyDictionary<Node, double> ComputeBetweennessCentrality(IGraph graph, ShortestPaths shortestPaths)
    {
        var pathCounts = new Dictionary<(Node, Node), int>(); // (source, target) -> number of paths
        var nodePathCounts = new Dictionary<(Node, Node), Dictionary<Node, int>>(); // (s, t) -> (v -> count)

        // Count all paths and node occurrences
        foreach (var path in shortestPaths.Paths)
        {
            var pathId = (path.Start, path.End);
            pathCounts[pathId] = pathCounts.GetValueOrDefault(pathId) + 1;

            if (!nodePathCounts.ContainsKey(pathId))
            {
                nodePathCounts[pathId] = new Dictionary<Node, int>();
            }

            foreach (var node in path.Skip(1).Select(e => e.Source))
            {
                nodePathCounts[pathId][node] = nodePathCounts[pathId].GetValueOrDefault(node) + 1;
            }
        }

        // Compute betweenness
        var betweenness = graph.Nodes.ToDictionary(n => n, _ => 0.0);
        foreach (var key in pathCounts.Keys)
        {
            var totalPaths = pathCounts[key];
            if (nodePathCounts.ContainsKey(key))
            {
                foreach (var kv in nodePathCounts[key])
                {
                    var nodeId = kv.Key;
                    var nodePaths = kv.Value;
                    betweenness[nodeId] += (double)nodePaths / totalPaths;
                }
            }
        }

        // Normalize by max number of edges
        var maxPairs = (graph.Nodes.Count - 1) * (graph.Nodes.Count - 2);
        if (maxPairs > 0)
        {
            foreach (var nodeId in betweenness.Keys)
            {
                betweenness[nodeId] /= maxPairs;
            }
        }

        return betweenness;
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