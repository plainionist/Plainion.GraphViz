using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Plainion.GraphViz.Modules.Metrics.Algorithms;

class ShortestPaths(IReadOnlyCollection<Path> paths)
{
    public IReadOnlyCollection<Path> Paths { get; } = paths;

    public IReadOnlyCollection<Path> Get(string sourceId, string targetId) =>
        Paths.Where(p => p.Start.Id == sourceId && p.End.Id == targetId).ToList();

    public override string ToString()
    {
        var sb = new StringBuilder();
        foreach (var path in Paths)
        {
            sb.AppendLine(path.ToString());
        }
        return sb.ToString();
    }
}
