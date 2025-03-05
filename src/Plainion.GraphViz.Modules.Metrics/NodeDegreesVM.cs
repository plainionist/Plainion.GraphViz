using Plainion.Graphs;

namespace Plainion.GraphViz.Modules.Metrics;

internal class NodeDegreesVM
{
    public required Node Model { get; init; }
    public required string Caption { get; init; }

    public required int In { get; init; }
    public required int Out { get; init; }
    public required int Total { get; init; }
}