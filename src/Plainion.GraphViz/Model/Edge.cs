using System;
using System.Diagnostics;

namespace Plainion.GraphViz.Model
{
    [Serializable]
    [DebuggerDisplay("{Source.Id} -> {Target.Id}")]
    public class Edge : IGraphItem, IEquatable<Edge>
    {
        public Edge(Node source, Node target)
        {
            Contract.RequiresNotNull(source, nameof(source));
            Contract.RequiresNotNull(target, nameof(target));

            Source = source;
            Target = target;

            Id = CreateId(source.Id, target.Id);
        }

        public string Id { get; }

        public Node Source { get; }
        public Node Target { get; }

        public bool Equals(Edge other) =>
            other != null && Id == other.Id;

        public override bool Equals(object obj) => Equals(obj as Edge);

        public override int GetHashCode() => Id.GetHashCode();

        public static string CreateId(string sourceId, string targetId)
        {
            return $"edge-from-{sourceId}-to-{targetId}";
        }
    }
}
