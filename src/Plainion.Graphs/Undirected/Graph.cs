using System.Collections.Generic;
using System.Linq;

namespace Plainion.Graphs.Undirected;

public static class Graph
{
    public static IEnumerable<(Node, Node)> Edges(this IEnumerable<Node> self)
        => self.WalkEdges().OrderBy(x => (x.Item1.Id, x.Item2.Id));

    private static IEnumerable<(Node, Node)> WalkEdges(this IEnumerable<Node> self)
    {
        if (self == null)
        {
            yield break;
        }

        var visited = new HashSet<string>();
        var coveredEdges = new HashSet<(string, string)>();
        var queue = new Queue<Node>();

        foreach (var startNode in self)
        {
            if (visited.Contains(startNode.Id))
            {
                continue;
            }

            queue.Enqueue(startNode);
            visited.Add(startNode.Id);

            while (queue.Count > 0)
            {
                var node = queue.Dequeue();

                foreach (var neighbor in node.Neighbors)
                {
                    var edgeKey = node.Id.CompareTo(neighbor.Id) < 0
                        ? (node.Id, neighbor.Id)
                        : (neighbor.Id, node.Id);

                    if (!coveredEdges.Contains(edgeKey))
                    {
                        yield return (node, neighbor);
                        coveredEdges.Add(edgeKey);
                    }

                    if (!visited.Contains(neighbor.Id))
                    {
                        visited.Add(neighbor.Id);
                        queue.Enqueue(neighbor);
                    }
                }
            }
        }
    }
}
