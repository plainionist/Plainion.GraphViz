using System.Collections.Generic;
using System.Linq;
using Plainion.Graphs;

namespace Plainion.GraphViz.Modules.Metrics.Algorithms;

class CycleFinder
{
    public static IReadOnlyCollection<Cycle> FindAllCycles(IGraph graph)
    {
        var unvisited = new HashSet<Node>(graph.Nodes);

        // nodes without edges can be ignored
        unvisited.RemoveWhere(n => n.In.Count == 0 || n.Out.Count == 0);

        var cycles = new List<Cycle>();

        while (unvisited.Count > 0)
        {
            var start = unvisited.First();
            unvisited.Remove(start);

            FindCycles(unvisited, start, new List<Edge>(), cycles);
        }

        return cycles;
    }

    private static void FindCycles(HashSet<Node> unvisited, Node current, List<Edge> edgePath, List<Cycle> cycles)
    {
        foreach (var edge in current.Out)
        {
            // Check for self-loop first
            if (edge.Source == edge.Target)
            {
                cycles.Add(new Cycle
                {
                    Start = edge.Source,
                    Edges = [edge]
                });
            }
            // Check for larger cycles
            else
            {
                var cycleStartIdx = edgePath.FindIndex(e => e.Source == edge.Target);

                // node exists in tracked path -> cycle detected
                if (cycleStartIdx >= 0)
                {
                    // ignore everything up to the cycle start
                    var cycleEdgePath = edgePath.Skip(cycleStartIdx).ToList();
                    // close the cycle with the start node
                    cycleEdgePath.Add(edge);

                    cycles.Add(new Cycle
                    {
                        Start = edge.Target,
                        Edges = cycleEdgePath
                    });
                }
                else if (unvisited.Contains(edge.Target))
                {
                    unvisited.Remove(edge.Target);
                    FindCycles(unvisited, edge.Target, new List<Edge>(edgePath) { edge }, cycles);
                }
            }
        }
    }
}