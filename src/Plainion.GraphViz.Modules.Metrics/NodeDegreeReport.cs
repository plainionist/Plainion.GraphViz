namespace Plainion.GraphViz.Modules.Metrics;

internal class NodeDegrees
{
    public required string Caption { get; init; }

    public required int In { get; init; }
    public required int Out { get; init; }
    public required int Total { get; init; }
}