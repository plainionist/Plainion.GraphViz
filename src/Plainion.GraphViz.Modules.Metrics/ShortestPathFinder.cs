using System.Collections.Generic;
using System.Threading.Tasks;
using Plainion.Graphs;

namespace Plainion.GraphViz.Modules.Metrics;

public class ShortestPathsResult(List<List<Edge>> paths)
{
    public List<List<Edge>> Paths { get; } = paths;
}

public static class ShortestPathsFinder
{
    public static ShortestPathsResult FindAllShortestPaths(IGraph graph)
    {
        var allPaths = new List<List<Edge>>();
        var lockObj = new object();

        Parallel.ForEach(graph.Nodes, source =>
        {
            var (visited, edges) = BFS(graph, source);
            var sourcePaths = new List<List<Edge>>();

            foreach (var target in graph.Nodes)
            {
                if (target.Id != source.Id && visited.Contains(target.Id))
                {
                    var path = ReconstructPath(source, target, edges);
                    if (path.Count > 0)
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

    private static (HashSet<string> visited, Dictionary<string, Edge> edges)
        BFS(IGraph graph, Node source)
    {
        var visited = new HashSet<string>();
        var edges = new Dictionary<string, Edge>(); // Edge leading to each node
        var queue = new Queue<Node>();

        visited.Add(source.Id);
        queue.Enqueue(source);

        while (queue.Count > 0)
        {
            var u = queue.Dequeue();
            foreach (var edge in u.Out)
            {
                var v = edge.Target;
                if (!visited.Contains(v.Id))
                {
                    visited.Add(v.Id);
                    edges[v.Id] = edge;
                    queue.Enqueue(v);
                }
            }
        }

        return (visited, edges);
    }

    private static List<Edge> ReconstructPath(Node start, Node end, Dictionary<string, Edge> edges)
    {
        var path = new List<Edge>();
        var currentId = end.Id;

        while (edges.ContainsKey(currentId))
        {
            var edge = edges[currentId];
            path.Add(edge);
            currentId = edge.Source.Id;
            if (currentId == start.Id) break;
        }

        path.Reverse();
        return path.Count > 0 && path[0].Source.Id == start.Id ? path : new List<Edge>();
    }
}