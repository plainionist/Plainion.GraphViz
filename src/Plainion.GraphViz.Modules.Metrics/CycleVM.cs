using System.Collections.Generic;

namespace Plainion.GraphViz.Modules.Metrics;

internal class CycleVM
{
    public required string Start { get; init; }
    public required IReadOnlyCollection<string> Path { get; init; }
}