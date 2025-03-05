using System.Collections.Generic;
using Plainion.GraphViz.Modules.Metrics.Algorithms;

namespace Plainion.GraphViz.Modules.Metrics;

internal class CycleVM
{
    public required Cycle Model { get; init; }
    public required string Start { get; init; }
    public required IReadOnlyCollection<string> Path { get; init; }
}