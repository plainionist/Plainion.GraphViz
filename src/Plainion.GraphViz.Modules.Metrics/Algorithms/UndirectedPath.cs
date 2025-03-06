using System;
using System.Collections.Generic;
using System.Linq;
using Plainion.Graphs.Undirected;

namespace Plainion.GraphViz.Modules.Metrics.Algorithms;

class UndirectedPath : IEquatable<UndirectedPath>
{
    private readonly IReadOnlyList<Node> myNodes;

    public UndirectedPath(IReadOnlyList<Node> nodes)
    {
        System.Contract.RequiresNotNull(nodes);

        myNodes = nodes;
    }

    public Node Start => myNodes[0];
    public Node End => myNodes[myNodes.Count - 1];
    public int Distance => myNodes.Count - 1; // Hops between nodes

    public override string ToString() =>
        string.Join(" - ", myNodes.Select(n => n.Id));

    public bool Equals(UndirectedPath other)
    {
        if (other == null) return false;
        if (ReferenceEquals(this, other)) return true;

        // order is not relevant!
        var thisSet = new HashSet<string>(myNodes.Select(n => n.Id));
        var otherSet = new HashSet<string>(other.myNodes.Select(n => n.Id));
        return thisSet.SetEquals(otherSet);
    }

    public override bool Equals(object obj) => Equals(obj as UndirectedPath);
    public override int GetHashCode() => HashCode.Combine(myNodes.OrderBy(x=>x.Id));
}
