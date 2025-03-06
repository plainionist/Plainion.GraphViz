using System.Collections;
using System.Collections.Generic;
using System.Text;
using Plainion.Graphs;

namespace Plainion.GraphViz.Modules.Metrics.Algorithms;

class Path : IEnumerable<Edge>
{
    private readonly IReadOnlyList<Edge> myPaths;

    public Path(IReadOnlyList<Edge> path)
    {
        System.Contract.RequiresNotNull(path);

        myPaths = path;
    }

    public Node Start => myPaths[0].Source;
    public Node End => myPaths[myPaths.Count - 1].Target;
    public int Distance => myPaths.Count;

    public IEnumerator<Edge> GetEnumerator() => myPaths.GetEnumerator();
    IEnumerator IEnumerable.GetEnumerator() => myPaths.GetEnumerator();
    public Edge this[int index] => myPaths[index];

    public override string ToString()
    {
        var sb = new StringBuilder();
        sb.Append($"{Start.Id}");
        foreach (var edge in myPaths)
        {
            sb.Append($" -> {edge.Target.Id}");
        }
        return sb.ToString();
    }
}
