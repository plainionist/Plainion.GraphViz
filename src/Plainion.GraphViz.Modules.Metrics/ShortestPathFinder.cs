using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Plainion.Graphs;

namespace Plainion.GraphViz.Modules.Metrics;

class Path : List<Edge> { }

class ShortestPaths(IReadOnlyCollection<Path> paths)
{
    public IReadOnlyCollection<Path> Paths { get; } = paths;

    public IReadOnlyCollection<Path> Get(string sourceId, string targetId) =>
        Paths.Where(p => p[0].Source.Id == sourceId && p.Last().Target.Id == targetId).ToList();
}

static class ShortestPathsFinder
{
    public static ShortestPaths FindAllShortestPaths(IGraph graph)
    {
        var allPaths = new List<Path>();
        var lockObj = new object();

        Parallel.ForEach(graph.Nodes, source =>
        {
            var (visited, edges) = BFS(graph, source);
            var sourcePaths = new List<Path>();

            foreach (var target in graph.Nodes)
            {
                if (target.Id != source.Id && visited.Contains(target.Id))
                {
                    var path = ReconstructPath(source, target, edges);
                    if (path.Count > 0)
                    {
                        sourcePaths.Add(path);
                    }
                }
            }

            lock (lockObj)
            {
                allPaths.AddRange(sourcePaths);
            }
        });

        return new ShortestPaths(allPaths);
    }

    private static (HashSet<string> visited, Dictionary<string, Edge> edges) BFS(IGraph graph, Node source)
    {
        var visited = new HashSet<string>();
        var edges = new Dictionary<string, Edge>(); // Edge leading to each node
        var queue = new Queue<Node>();

        visited.Add(source.Id);
        queue.Enqueue(source);

        while (queue.Count > 0)
        {
            var node = queue.Dequeue();
            foreach (var edge in node.Out)
            {
                if (!visited.Contains(edge.Target.Id))
                {
                    visited.Add(edge.Target.Id);
                    edges[edge.Target.Id] = edge;
                    queue.Enqueue(edge.Target);
                }
            }
        }

        return (visited, edges);
    }

    private static Path ReconstructPath(Node source, Node target, Dictionary<string, Edge> edges)
    {
        var path = new Path();
        var currentId = target.Id;

        while (edges.ContainsKey(currentId))
        {
            var edge = edges[currentId];
            path.Add(edge);
            currentId = edge.Source.Id;

            if (currentId == source.Id)
            {
                break;
            }
        }

        path.Reverse();

        return path.Count > 0 && path[0].Source.Id == source.Id ? path : [];
    }
}