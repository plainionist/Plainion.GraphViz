using System;
using System.Diagnostics;

namespace Plainion.GraphViz.Model
{
    [Serializable]
    [DebuggerDisplay("{Source.Id} -> {Target.Id}")]
    public class Edge : IGraphItem, IEquatable<Edge>
    {
        public Edge(Node source, Node target)
            : this(source, target, 1)
        {
        }

        public Edge(Node source, Node target, int weight)
        {
            Contract.RequiresNotNull(source, nameof(source));
            Contract.RequiresNotNull(target, nameof(target));
            Contract.Requires(weight >= 0, "Weight must be >= 0");

            Source = source;
            Target = target;
            Weight = weight;

            Id = CreateId(source.Id, target.Id);
        }

        public string Id { get; }
        /// <summary>
        /// Default: 1
        /// </summary>
        public int Weight { get; }

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
