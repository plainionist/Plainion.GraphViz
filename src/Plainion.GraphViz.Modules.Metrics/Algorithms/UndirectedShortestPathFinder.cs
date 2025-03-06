using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Plainion.Graphs.Undirected;

namespace Plainion.GraphViz.Modules.Metrics.Algorithms;

class UndirectedPath : IEquatable<UndirectedPath>
{
    private readonly IReadOnlyList<Node> myNodes;

    public UndirectedPath(IReadOnlyList<Node> nodes)
    {
        System.Contract.RequiresNotNull(nodes);
        myNodes = nodes;
    }

    public Node Start => myNodes[0];
    public Node End => myNodes[myNodes.Count - 1];
    public int Distance => myNodes.Count - 1; // Hops between nodes

    public override string ToString()
    {
        return string.Join(" - ", myNodes.Select(n => n.Id));
    }

    public bool Equals(UndirectedPath other)
    {
        if (other == null) return false;
        if (ReferenceEquals(this, other)) return true;
        if (Distance != other.Distance) return false;

        // Undirected: A - B == B - A
        return (Start.Id == other.Start.Id && End.Id == other.End.Id) ||
               (Start.Id == other.End.Id && End.Id == other.Start.Id);
    }

    public override bool Equals(object obj) => Equals(obj as UndirectedPath);

    public override int GetHashCode()
    {
        var minId = Math.Min(Start.Id.GetHashCode(), End.Id.GetHashCode());
        var maxId = Math.Max(Start.Id.GetHashCode(), End.Id.GetHashCode());
        return HashCode.Combine(minId, maxId, Distance);
    }
}

class ShortestUndirectedPaths
{
    public IReadOnlyCollection<UndirectedPath> Paths { get; }

    public ShortestUndirectedPaths(IReadOnlyCollection<UndirectedPath> paths)
    {
        Paths = paths.Distinct().ToList(); // Remove duplicates
    }

    public IReadOnlyCollection<UndirectedPath> Get(string sourceId, string targetId) =>
        Paths.Where(p => p.Start.Id == sourceId && p.End.Id == targetId).ToList();

    public override string ToString() => string.Join("\n", Paths);
}

static class UndirectedShortestPathsFinder
{
    public static ShortestUndirectedPaths FindAllShortestPaths(IReadOnlyCollection<Node> graph)
    {
        var allPaths = graph
            //.AsParallel()
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
