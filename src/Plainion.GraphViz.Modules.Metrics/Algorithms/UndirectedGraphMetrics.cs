using System.Collections.Generic;
using System.Linq;
using Plainion.Graphs.Undirected;
using Plainion.GraphViz.Presentation;

namespace Plainion.GraphViz.Modules.Metrics.Algorithms;

internal class UndirectedGraphMetrics
{
    /// <summary>
    /// Measures how close a node is to all other nodes via shortest paths
    /// https://en.wikipedia.org/wiki/Closeness_centrality
    /// </summary>
    public static IReadOnlyCollection<GraphItemMeasurement<Node>> ComputeClosenessCentrality(IReadOnlyCollection<Node> graph, ShortestUndirectedPaths shortestPaths)
    {
        // Build distance map for all unique pairs
        var distances = new Dictionary<(string, string), int>();
        foreach (var path in shortestPaths.Paths)
        {
            var sourceId = path.Start.Id;
            var targetId = path.End.Id;
            var key = sourceId.CompareTo(targetId) < 0 ? (sourceId, targetId) : (targetId, sourceId);
            if (!distances.ContainsKey(key)) // Avoid duplicates
            {
                distances[key] = path.Distance;
            }
        }

        // Compute closeness for each node
        return graph.Select(node =>
        {
            // Sum distances to all other nodes
            var totalSum = distances
                .Where(kv => kv.Key.Item1 == node.Id || kv.Key.Item2 == node.Id)
                .Sum(kv => kv.Value);

            var absolute = totalSum > 0 ? 1.0 / totalSum : 0.0;
            var normalized = totalSum > 0 ? (double)(graph.Count - 1) / totalSum : 0.0;

            return new GraphItemMeasurement<Node>
            {
                Owner = node,
                Absolute = absolute,
                Normalized = normalized
            };
        }).ToList();
    }
}