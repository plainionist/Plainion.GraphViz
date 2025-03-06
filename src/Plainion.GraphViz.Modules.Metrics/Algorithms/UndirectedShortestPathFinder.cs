using System.Collections.Generic;
using System.Linq;
using Plainion.Graphs.Undirected;

namespace Plainion.GraphViz.Modules.Metrics.Algorithms;

static class UndirectedShortestPathsFinder
{
    public static ShortestUndirectedPaths FindAllShortestPaths(IReadOnlyCollection<Node> graph)
    {
        var allPaths = graph
            .AsParallel()
            .SelectMany(x => BFSAllPaths(x, graph))
            .ToList();

        return new ShortestUndirectedPaths(allPaths);
    }

    private static List<UndirectedPath> BFSAllPaths(Node source, IReadOnlyCollection<Node> graph)
    {
        var paths = new Dictionary<string, UndirectedPath>(); // Target ID -> Shortest Path
        var queue = new Queue<(Node node, List<Node> path, int dist)>();
        var distances = graph.ToDictionary(n => n.Id, _ => int.MaxValue);

        queue.Enqueue((source, new List<Node> { source }, 0));
        distances[source.Id] = 0;

        while (queue.Count > 0)
        {
            var (current, currentPath, currentDist) = queue.Dequeue();

            foreach (var next in current.Neighbors)
            {
                if (currentPath.Contains(next)) continue; // Prevent cycles

                var newDist = currentDist + 1;
                if (newDist < distances[next.Id])
                {
                    var newPath = new List<Node>(currentPath) { next };
                    distances[next.Id] = newDist;
                    paths[next.Id] = new UndirectedPath(newPath);
                    queue.Enqueue((next, newPath, newDist));
                }
            }
        }

        return paths.Values.ToList();
    }
}
