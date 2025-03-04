using Plainion.Graphs;

namespace Plainion.GraphViz.Modules.Metrics.Algorithms;

class BetweennessCentrality<T> where T : IGraphItem
{
    public required T Owner { get; init; }
    public required double Absolute { get; init; }
    public required double Normalized { get; init; }
}