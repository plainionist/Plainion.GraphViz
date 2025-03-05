using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Plainion.Collections;
using Plainion.Graphs.Undirected;

namespace Plainion.GraphViz.Modules.Metrics.Algorithms;

class UndirectedPath : IEnumerable<Edge>, IEquatable<UndirectedPath>
{
    private readonly IReadOnlyList<Edge> myPaths;

    public UndirectedPath(IReadOnlyList<Edge> path)
    {
        System.Contract.RequiresNotNull(path);

        myPaths = path;
    }

    public Node Start => myPaths[0].Source;
    public Node End => myPaths[myPaths.Count - 1].Target;

    public int Distance => myPaths.Count;
    public IEnumerator<Edge> GetEnumerator() => myPaths.GetEnumerator();
    IEnumerator IEnumerable.GetEnumerator() => myPaths.GetEnumerator();
    public Edge this[int index] => myPaths[index];

    public override string ToString()
    {
        var sb = new StringBuilder();
        sb.Append($"{Start.Id}");
        foreach (var edge in myPaths)
        {
            sb.Append($" - {edge.Target.Id}");
        }
        return sb.ToString();
    }

    public bool Equals(UndirectedPath other)
    {
        if (other == null) return false;
        if (ReferenceEquals(this, other)) return true;
        if (Distance != other.Distance) return false;

        // TODO: performance
        return ToString() == other.ToString();
    }

    public override bool Equals(object obj) => Equals(obj as UndirectedPath);

    public override int GetHashCode() =>
        // TODO: performance
        ToString().GetHashCode();

}

class ShortestUndirectedPaths(IReadOnlyCollection<UndirectedPath> paths)
{
    // with undirected edges we could have duplicates!
    public IReadOnlyCollection<UndirectedPath> Paths { get; } = paths.Distinct().ToList();

    public IReadOnlyCollection<UndirectedPath> Get(string sourceId, string targetId) =>
        Paths.Where(p => p.Start.Id == sourceId && p.End.Id == targetId).ToList();

    public override string ToString()
    {
        var sb = new StringBuilder();
        foreach (var path in Paths)
        {
            sb.AppendLine(path.ToString());
        }
        return sb.ToString();
    }
}

static class UndirectedShortestPathsFinder
{
    public static ShortestUndirectedPaths FindAllShortestPaths(Graph graph)
    {
        var allPaths = new List<UndirectedPath>();
        var lockObj = new object();

        Parallel.ForEach(graph.Nodes, source =>
        {
            var sourcePaths = BFSAllPaths(graph, source);
            lock (lockObj)
            {
                allPaths.AddRange(sourcePaths);
            }
        });

        var result = new ShortestUndirectedPaths(allPaths);
        Console.WriteLine(result);
        return result;
    }

    private static List<UndirectedPath> BFSAllPaths(Graph graph, Node source)
    {
        var paths = new List<UndirectedPath>();
        var queue = new Queue<(Node node, List<Edge> path, int dist)>();
        var distances = graph.Nodes.ToDictionary(n => n.Id, _ => int.MaxValue);
        var allPathsToNode = new Dictionary<string, List<List<Edge>>>();

        queue.Enqueue((source, new List<Edge>(), 0));
        distances[source.Id] = 0;
        allPathsToNode[source.Id] = new List<List<Edge>> { new List<Edge>() };

        while (queue.Count > 0)
        {
            var (current, currentPath, currentDist) = queue.Dequeue();

            foreach (var edge in current.Edges)
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
                paths.AddRange(allPathsToNode[target.Id].Select(x => new UndirectedPath(x)));
            }
        }

        return paths;
    }
}