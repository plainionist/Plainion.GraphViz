using System.Collections.Generic;
using System.Linq;
using Plainion.Graphs;

namespace Plainion.GraphViz.Modules.Metrics;

public class CycleDetectionAlgorithm
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
        foreach (var edge in current.In) 
        {
            var sourceNodeIdx = path.IndexOf(edge.Source);

            // node exists in tracked path -> cycle detected
            if (sourceNodeIdx >= 0) 
            {
                // ignore everything up to the cycle start
                var cycleNodes = path.Skip(sourceNodeIdx).ToList();

                // reverse to match forward order
                var cycle = cycleNodes.Select(n => n).Reverse().ToList();
                cycle.Add(cycleNodes.Last()); // Close the cycle with the start node
                
                cycles.Add(cycle);
            }
            else if (unvisited.Contains(edge.Source)) // Continue exploration
            {
                unvisited.Remove(edge.Source);
                var newPath = new List<Node>(path) { edge.Source };
                FindCycles(unvisited, edge.Source, newPath, cycles);
            }
        }
    }
}
