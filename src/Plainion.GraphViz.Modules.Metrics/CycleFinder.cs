using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Plainion.Graphs;

namespace Plainion.GraphViz.Modules.Metrics;

class Cycle
{
    public required Node Start { get; init; }
    public required IReadOnlyCollection<Node> Path { get; init; }
}

class CycleFinder
{
    public static List<Cycle> FindAllCycles(IGraph graph)
    {
        var unvisited = new HashSet<Node>(graph.Nodes);

        // nodes without edges can be ignored
        unvisited.RemoveWhere(n => n.In.Count == 0 || n.Out.Count == 0);

        var cycles = new List<Cycle>();

        while (unvisited.Count > 0)
        {
            var start = unvisited.First();
            unvisited.Remove(start);

            FindCycles(unvisited, start, [start], cycles);
        }

        return cycles;
    }

    private static void FindCycles(HashSet<Node> unvisited, Node current, List<Node> path, List<Cycle> cycles)
    {
        foreach (var edge in current.Out)
        {
            var targetNodeIdx = path.IndexOf(edge.Target);

            // node exists in tracked path -> cycle detected
            if (targetNodeIdx >= 0)
            {
                var cycleStartNode = path[targetNodeIdx];

                // ignore everything up to the cycle start
                var cyclePath = path.Skip(targetNodeIdx + 1).ToList();
                // close the cycle with the start node
                cyclePath.Add(cycleStartNode);

                cycles.Add(new Cycle
                {
                    Start = cycleStartNode,
                    Path = cyclePath
                });
            }
            else if (unvisited.Contains(edge.Target))
            {
                // Continue walking the path
                unvisited.Remove(edge.Target);
                FindCycles(unvisited, edge.Target, new List<Node>(path) { edge.Target }, cycles);
            }
        }
    }
}
