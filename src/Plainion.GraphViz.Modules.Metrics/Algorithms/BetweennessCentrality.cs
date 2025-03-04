namespace Plainion.GraphViz.Modules.Metrics.Algorithms;

class BetweennessCentrality
{
    public required string OwnerId { get; init; }
    public required double Absolute { get; init; }
    public required double Normalized { get; init; }
}