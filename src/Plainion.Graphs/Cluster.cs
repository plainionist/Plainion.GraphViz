using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace Plainion.Graphs;

[Serializable]
[DebuggerDisplay("{Id}")]
public class Cluster : IGraphItem, IEquatable<Cluster>
{
    public Cluster(string id, IEnumerable<Node> nodes)
    {
        Contract.RequiresNotNullNotEmpty(id, nameof(id));

        Id = id;

        Nodes = nodes.ToList();
    }

    public string Id { get; }

    public IReadOnlyCollection<Node> Nodes { get; }

    public bool Equals(Cluster other) => other != null && Id == other.Id;
    public override bool Equals(object obj) => Equals(obj as Cluster);
    public override int GetHashCode() => Id.GetHashCode();
}
