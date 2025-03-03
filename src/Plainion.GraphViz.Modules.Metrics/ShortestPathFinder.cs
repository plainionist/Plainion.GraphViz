using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Plainion.Graphs;

namespace Plainion.GraphViz.Modules.Metrics;

public class ShortestPathsResult
{
    public List<List<Edge>> Paths { get; }

    public ShortestPathsResult(List<List<Edge>> paths)
    {
        Paths = paths;
    }

    public static int GetDistance(List<Edge> path) => path.Sum(e => e.Weight);
}

public static class ShortestPathsFinder
{
    public static ShortestPathsResult FindAllShortestPaths(IGraph graph)
    {
        var allPaths = new List<List<Edge>>();
        var lockObj = new object();

        Parallel.ForEach(graph.Nodes, source =>
        {
            var (dist, prev, edges) = Dijkstra(graph, source);
            var sourcePaths = new List<List<Edge>>();

            foreach (var target in graph.Nodes)
            {
                if (target.Id != source.Id && dist.TryGetValue(target.Id, out int distance) && distance != int.MaxValue)
                {
                    var path = ReconstructPath(source, target, prev, edges);
                    if (path.Count > 0) // Only add non-empty paths
                        sourcePaths.Add(path);
                }
            }

            lock (lockObj)
            {
                allPaths.AddRange(sourcePaths);
            }
        });

        return new ShortestPathsResult(allPaths);
    }

    private static (Dictionary<string, int> dist, Dictionary<string, string> prev, Dictionary<string, Edge> edges) Dijkstra(IGraph graph, Node source)
    {
        var dist = graph.Nodes.ToDictionary(n => n.Id, _ => int.MaxValue);
        var prev = new Dictionary<string, string>();
        var edges = new Dictionary<string, Edge>(); // Tracks edge leading to each node
        var pq = new PriorityQueue<string, int>();

        dist[source.Id] = 0;
        pq.Enqueue(source.Id, 0);

        while (pq.Count > 0)
        {
            var u = pq.Dequeue();
            var uNode = graph.FindNode(u);

            foreach (var edge in uNode.Out)
            {
                var v = edge.Target.Id;
                int alt = dist[u] == int.MaxValue ? int.MaxValue : dist[u] + edge.Weight;

                if (alt < dist[v])
                {
                    dist[v] = alt;
                    prev[v] = u;
                    edges[v] = edge; // Store the edge used to reach v
                    pq.Enqueue(v, alt);
                }
            }
        }

        return (dist, prev, edges);
    }

    private static List<Edge> ReconstructPath(Node start, Node end, Dictionary<string, string> prev, Dictionary<string, Edge> edges)
    {
        var path = new List<Edge>();
        var currentId = end.Id;

        while (prev.ContainsKey(currentId))
        {
            var prevId = prev[currentId];
            if (edges.TryGetValue(currentId, out var edge))
            {
                path.Add(edge);
            }
            currentId = prevId;
            if (currentId == start.Id) break;
        }

        path.Reverse();
        return path.Count > 0 && path[0].Source.Id == start.Id ? path : new List<Edge>();
    }
}

