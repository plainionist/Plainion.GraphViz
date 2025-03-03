using System.Collections.Generic;
using System.Linq;
using Plainion.Graphs;

namespace Plainion.GraphViz.Modules.Metrics;

class CycleDetectionAlgorithm
{
    public List<List<Node>> Compute(IGraph graph)
    {
        var unvisited = new HashSet<Node>(graph.Nodes);

        // nodes without edges can be ignored
        unvisited.RemoveWhere(n => n.In.Count == 0 || n.Out.Count == 0);

        var cycles = new List<List<Node>>();

        while (unvisited.Count > 0)
        {
            var start = unvisited.First();
            unvisited.Remove(start);

            FindCycles(unvisited, start, [start], cycles);
        }

        return cycles;
    }

    private static void FindCycles(HashSet<Node> unvisited, Node current, List<Node> path, List<List<Node>> cycles)
    {
        foreach (var edge in current.Out)
        {
            var targetNodeIdx = path.IndexOf(edge.Target);

            // node exists in tracked path -> cycle detected
            if (targetNodeIdx >= 0)
            {
                // ignore everything up to the cycle start
                var cycleNodes = path.Skip(targetNodeIdx).ToList();

                // close the cycle with the start node
                cycleNodes.Add(cycleNodes.First());

                cycles.Add(cycleNodes);
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
