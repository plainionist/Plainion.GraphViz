using System.Collections.Generic;

namespace Plainion.GraphViz.Modules.Metrics
{
    public class PathwaysVM
    {
        public required int Diameter { get; init; }
        public required double AveragePathLength { get; init; }
        public required IReadOnlyCollection<KeyValuePair<string, double>> BetweennessCentrality { get; init; }
    }
}