using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace Plainion.Graphs.Undirected;

[DebuggerDisplay("{Id}")]
public class Node : IGraphItem, IEquatable<Node>
{
    public Node(string id)
    {
        System.Contract.RequiresNotNullNotEmpty(id);

        Id = id;

        Neighbors = [];
    }

    public string Id { get; }

    public IList<Node> Neighbors { get; }

    public bool Equals(Node other) => other != null && Id == other.Id;
    public override bool Equals(object obj) => Equals(obj as Node);
    public override int GetHashCode() => Id.GetHashCode();
}
