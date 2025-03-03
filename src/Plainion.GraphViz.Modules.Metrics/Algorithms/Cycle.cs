using System.Collections.Generic;
using Plainion.Graphs;

namespace Plainion.GraphViz.Modules.Metrics.Algorithms;

class Cycle
{
    public required Node Start { get; init; }
    public required IReadOnlyCollection<Node> Path { get; init; }
}
