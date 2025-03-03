using System.Collections.Generic;

namespace Plainion.GraphViz.Modules.Metrics
{
    public class GraphCycle
    {
        public required string Start { get; init; }
        public required IReadOnlyCollection<string> Path { get; init; }
    }
}