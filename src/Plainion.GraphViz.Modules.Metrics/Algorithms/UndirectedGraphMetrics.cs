using System.Collections.Generic;
using System.Linq;
using Plainion.Graphs;

namespace Plainion.GraphViz.Modules.Metrics.Algorithms;

internal class UndirectedGraphMetrics
{
    /// <summary>
    /// Measures how close a node is to all other nodes via shortest paths
    /// https://en.wikipedia.org/wiki/Closeness_centrality
    /// </summary>
    public static IReadOnlyCollection<GraphItemMeasurement<Node>> ComputeClosenessCentrality(IGraph graph, ShortestPaths shortestPaths)
    {
        var distances = new Dictionary<Node, Dictionary<Node, int>>(); // v -> (u -> distance)
        foreach (var node in graph.Nodes)
        {
            distances[node] = [];
        }

        // Aggregate shortest path distances
        foreach (var path in shortestPaths.Paths)
        {
            if (!distances[path.Start].ContainsKey(path.End))
            {
                distances[path.Start][path.End] = path.Distance;
            }
        }

        // Compute closeness for each node
        return graph.Nodes.Select(node =>
        {
            var sumDistances = distances[node].Values.Sum();
            return new GraphItemMeasurement<Node>
            {
                Owner = node,
                Absolute = sumDistances > 0 ? 1.0 / sumDistances : 0.0,
                Normalized = sumDistances > 0 ? (double)(graph.Nodes.Count - 1) / sumDistances : 0.0
            };
        }).ToList();
    }

}
