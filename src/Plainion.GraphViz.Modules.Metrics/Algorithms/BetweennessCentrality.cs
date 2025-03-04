using Plainion.Graphs;

namespace Plainion.GraphViz.Modules.Metrics.Algorithms;

class BetweennessCentrality
{
    public required Node Node { get; init; }
    public required double Absolute { get; init; }
    public required double Normalized { get; init; }
}