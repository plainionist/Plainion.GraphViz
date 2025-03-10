﻿using Plainion.Graphs;

namespace Plainion.GraphViz.Modules.Metrics;

class GraphItemMeasurementVM
{
    public required IGraphItem Model { get; init; }
    public required string Caption { get; init; }
    public required double Absolute { get; init; }
    public required double Normalized { get; init; }
}
