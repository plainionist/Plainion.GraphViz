using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace Plainion.GraphViz.Model
{
    [Serializable]
    [DebuggerDisplay("{Id}")]
    public class Node : IGraphItem, IEquatable<Node>
    {
        public Node(string id)
        {
            Contract.RequiresNotNullNotEmpty(id, nameof(id));

            Id = id;

            In = new List<Edge>();
            Out = new List<Edge>();
        }

        public string Id { get; }

        public IList<Edge> In { get; }
        public IList<Edge> Out { get; }

        public bool Equals(Node other) =>
            other != null && Id == other.Id;

        public override bool Equals(object obj) => Equals(obj as Node);

        public override int GetHashCode() => Id.GetHashCode();
    }
}
