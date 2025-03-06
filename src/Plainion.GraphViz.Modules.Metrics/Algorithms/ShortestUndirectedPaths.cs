using System.Collections.Generic;
using System.Linq;

namespace Plainion.GraphViz.Modules.Metrics.Algorithms;

class ShortestUndirectedPaths
{
    public IReadOnlyCollection<UndirectedPath> Paths { get; }

    public ShortestUndirectedPaths(IReadOnlyCollection<UndirectedPath> paths)
    {
        Paths = paths.Distinct().ToList(); // Remove duplicates
    }

    public IReadOnlyCollection<UndirectedPath> Get(string sourceId, string targetId) =>
        Paths.Where(p => p.Start.Id == sourceId && p.End.Id == targetId).ToList();

    public override string ToString() => string.Join("\n", Paths);
}
