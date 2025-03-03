using System.Collections;
using System.Collections.Generic;
using Plainion.Graphs;

namespace Plainion.GraphViz.Modules.Metrics.Algorithms;

class Path : IReadOnlyList<Edge>
{
    private readonly IReadOnlyList<Edge> myPaths;

    public Path(IReadOnlyList<Edge> path)
    {
        System.Contract.RequiresNotNull(path);

        myPaths = path;
    }

    public Node Start => myPaths[0].Source;
    public Node End => myPaths[myPaths.Count - 1].Target;

    public int Count => myPaths.Count;
    public IEnumerator<Edge> GetEnumerator() => myPaths.GetEnumerator();
    IEnumerator IEnumerable.GetEnumerator() => myPaths.GetEnumerator();
    public Edge this[int index] => myPaths[index];
}
