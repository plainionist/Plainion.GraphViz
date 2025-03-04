namespace Plainion.GraphViz.Modules.Metrics;

class BetweennessVM
{
    public required string Node { get; init; }
    public required double Absolute { get; init; }
    public required double Normalized { get; init; }
}
