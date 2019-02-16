using System;

namespace Plainion.GraphViz.Model
{
    [Serializable]
    public class Edge : AbstractGraphItem
    {
        public Edge(Node source, Node target)
            : base(CreateId(source, target))
        {
            Source = source;
            Target = target;
        }

        private static string CreateId(Node source, Node target)
        {
            Contract.RequiresNotNull(source, nameof(source));
            Contract.RequiresNotNull(target, nameof(target));

            return CreateId(source.Id, target.Id);
        }

        public Node Source { get; private set; }
        public Node Target { get; private set; }

        public static string CreateId(string sourceId, string targetId)
        {
            return $"edge-from-{sourceId}-to-{targetId}";
        }
    }
}
