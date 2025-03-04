﻿using System.Collections.Generic;

namespace Plainion.GraphViz.Modules.Metrics;

class PathwaysVM
{
    public required int Diameter { get; init; }
    public required double AveragePathLength { get; init; }
    public required IReadOnlyCollection<BetweennessVM> BetweennessCentrality { get; init; }
    public required IReadOnlyCollection<BetweennessVM> EdgeBetweenness { get; init; }
}