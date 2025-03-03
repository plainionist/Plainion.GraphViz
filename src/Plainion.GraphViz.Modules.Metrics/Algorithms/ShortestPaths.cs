using System.Collections.Generic;
using System.Linq;

namespace Plainion.GraphViz.Modules.Metrics.Algorithms;

class ShortestPaths(IReadOnlyCollection<Path> paths)
{
    public IReadOnlyCollection<Path> Paths { get; } = paths;

    public IReadOnlyCollection<Path> Get(string sourceId, string targetId) =>
        Paths.Where(p => p[0].Source.Id == sourceId && p.Last().Target.Id == targetId).ToList();
}
