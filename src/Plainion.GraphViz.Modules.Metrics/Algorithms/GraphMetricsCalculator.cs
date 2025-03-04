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
                OwnerId = x.Key.Id,
                Absolute = x.Value,
                Normalized = maxPairs == 0 ? 0.0 : x.Value / maxPairs,
            })
            .ToList();
    }

    /// <summary>
    /// Measures how often an edge is part of the shortest path between nodes
    /// https://en.wikipedia.org/wiki/Betweenness_centrality#Edge_betweenness_centrality
    /// </summary>
    public static IReadOnlyCollection<BetweennessCentrality> ComputeEdgeBetweenness(IGraph graph, ShortestPaths shortestPaths)
    {
        // (source, target) -> number of paths
        var pathCounts = new Dictionary<(Node, Node), int>();
        var edgePathCounts = new Dictionary<(Node, Node), Dictionary<Edge, int>>(); // (s, t) -> (e -> count)
        var maxPairs = (graph.Nodes.Count - 1) * (graph.Nodes.Count - 2);

        // Count all shortest paths between 2 nodes
        // and how often each edge is part of each path
        foreach (var path in shortestPaths.Paths)
        {
            var pathId = (path[0].Source, path.Last().Target);
            pathCounts[pathId] = pathCounts.GetValueOrDefault(pathId) + 1;

            if (!edgePathCounts.ContainsKey(pathId))
            {
                edgePathCounts[pathId] = [];
            }

            foreach (var edge in path)
            {
                edgePathCounts[pathId][edge] = edgePathCounts[pathId].GetValueOrDefault(edge) + 1;
            }
        }

        // Compute edge betweenness
        var betweenness = graph.Edges.ToDictionary(e => e, _ => 0.0);
        foreach (var entry in edgePathCounts)
        {
            foreach (var edgeCount in entry.Value)
            {
                betweenness[edgeCount.Key] += (double)edgeCount.Value / pathCounts[entry.Key];
            }
        }

        return betweenness
            .Select(x => new BetweennessCentrality
            {
                OwnerId = x.Key.Id,
                Absolute = x.Value,
                Normalized = maxPairs > 0 ? x.Value / maxPairs : 0.0
            })
            .ToList();
    }
}