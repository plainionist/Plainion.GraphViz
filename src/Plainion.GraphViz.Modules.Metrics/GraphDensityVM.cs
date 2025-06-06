﻿namespace Plainion.GraphViz.Modules.Metrics;

internal class GraphDensityVM
{
    public required int NodeCount { get; init; }
    public required int EdgeCount { get; init; }
    public required double Density { get; init; }
}