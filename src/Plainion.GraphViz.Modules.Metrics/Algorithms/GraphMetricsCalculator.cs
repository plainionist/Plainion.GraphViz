using System.Collections.Generic;
using System.Linq;
using Plainion.Graphs;

namespace Plainion.GraphViz.Modules.Metrics.Algorithms;

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

    /// <summary>
    /// Measures how often a node is a member of the shortest path between other nodes
    /// </summary>
    public static Dictionary<string, double> ComputeBetweennessCentrality(IGraph graph, ShortestPaths shortestPaths)
    {
        var betweenness = graph.Nodes.ToDictionary(n => n.Id, _ => 0.0);
        var maxPairs = graph.Nodes.Count * (graph.Nodes.Count - 1);

        foreach (var path in shortestPaths.Paths)
        {
            // skip start and end, ignore target nodes as target of one edge is the source of the next
            foreach(var nodeId in path.Skip(1).Select(e => e.Source.Id))
            {
                betweenness[nodeId] += 1.0; // Add 1 for each path it’s on
            }
        }

        // Normalize by max possible directed pairs
        if (maxPairs > 0)
        {
            foreach (var nodeId in betweenness.Keys)
            {
                betweenness[nodeId] /= maxPairs;
            }
        }

        return betweenness;
    }
}