using System.Collections.Generic;
using System.Linq;
using Plainion.Graphs;

namespace Plainion.GraphViz.Modules.Metrics;

static class GraphMetricsCalculator
{
    /// <summary>
    /// Ratio of actual edges to the maximum possible edges in the graph
    /// </summary>
    public static GraphDensity ComputeGraphDensity(IGraph graph) =>
        new()
        {
            NodeCount = graph.Nodes.Count,
            EdgeCount = graph.Edges.Count,
            Density = graph.Edges.Count == 0 ? 0 : (double)graph.Edges.Count / (graph.Nodes.Count * (graph.Nodes.Count - 1))
        };

    /// <summary>
    /// The longest shortest path between any two nodes in the graph
    /// https://en.wikipedia.org/wiki/Diameter_(graph_theory)
    /// </summary>
    public static int ComputeDiameter(ShortestPaths shortestPaths) =>
        shortestPaths.Paths.Count == 0 ? 0 : shortestPaths.Paths.Max(path => path.Count);

    /// <summary>
    /// The average shortest path length between two nodes in the graph.
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

    public static Dictionary<string, double> ComputeBetweennessCentrality(IGraph graph, ShortestPaths shortestPaths)
    {
        var betweenness = graph.Nodes.ToDictionary(n => n.Id, _ => 0.0);
        var pairCount = new Dictionary<(string, string), int>(); // (source, target) -> path count

        // Count paths between each pair
        foreach (var path in shortestPaths.Paths)
        {
            var source = path[0].Source.Id;
            var target = path.Last().Target.Id;
            var key = (source, target);
            pairCount[key] = pairCount.GetValueOrDefault(key) + 1;
        }

        // Calculate betweenness for each node
        foreach (var path in shortestPaths.Paths)
        {
            var source = path[0].Source.Id;
            var target = path.Last().Target.Id;
            var pathCount = pairCount[(source, target)];

            // Increment betweenness for intermediate nodes
            foreach (var edge in path)
            {
                var nodeId = edge.Source.Id;
                if (nodeId != source && nodeId != target) // Exclude endpoints
                {
                    betweenness[nodeId] += 1.0 / pathCount; // Fraction of paths through this node
                }
            }
        }

        // Normalize by number of pairs (n-1)(n-2) for directed graph
        var n = graph.Nodes.Count;
        var normalization = n > 2 ? (n - 1) * (n - 2) : 1;
        foreach (var nodeId in betweenness.Keys)
        {
            betweenness[nodeId] /= normalization;
        }

        return betweenness;
    }
}