using System.Collections.Generic;
using System.Linq;
using Plainion.Graphs;

namespace Plainion.GraphViz.Modules.Metrics.Algorithms;

public class Cycle
{
    public required Node Start { get; init; }
    public required IReadOnlyCollection<Edge> Edges { get; init; }

    // Start node is not repeated and only occurs once
    public IEnumerable<Node> Path => Edges.Select(x => x.Target);
}
