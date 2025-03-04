using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Plainion.Graphs;

namespace Plainion.GraphViz.Modules.Metrics.Algorithms;

static class ShortestPathsFinder
{
    public static ShortestPaths FindAllShortestPaths(IGraph graph)
    {
        var allPaths = new List<Path>();
        var lockObj = new object();

        Parallel.ForEach(graph.Nodes, source =>
        {
            var sourcePaths = BFSAllPaths(graph, source);
            lock (lockObj)
            {
                allPaths.AddRange(sourcePaths);
            }
        });

        return new ShortestPaths(allPaths);
    }

    private static List<Path> BFSAllPaths(IGraph graph, Node source)
    {
        var paths = new List<Path>();
        var queue = new Queue<(Node node, List<Edge> path, int dist)>();
        var distances = graph.Nodes.ToDictionary(n => n.Id, _ => int.MaxValue);
        var allPathsToNode = new Dictionary<string, List<List<Edge>>>();

        queue.Enqueue((source, new List<Edge>(), 0));
        distances[source.Id] = 0;
        allPathsToNode[source.Id] = new List<List<Edge>> { new List<Edge>() };

        while (queue.Count > 0)
        {
            var (current, currentPath, currentDist) = queue.Dequeue();

            foreach (var edge in current.Out)
            {
                var next = edge.Target;
                var newPath = new List<Edge>(currentPath) { edge };
                var newDist = currentDist + 1;

                if (newDist < distances[next.Id])
                {
                    distances[next.Id] = newDist;
                    allPathsToNode[next.Id] = new List<List<Edge>> { newPath };
                    queue.Enqueue((next, newPath, newDist));
                }
                else if (newDist == distances[next.Id])
                {
                    allPathsToNode[next.Id].Add(newPath);
                }
            }
        }

        foreach (var target in graph.Nodes)
        {
            if (target.Id != source.Id && allPathsToNode.ContainsKey(target.Id))
            {
                paths.AddRange(allPathsToNode[target.Id].Select(x => new Path(x)));
            }
        }

        return paths;
    }
}