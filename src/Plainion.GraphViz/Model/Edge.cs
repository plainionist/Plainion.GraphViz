using System;

namespace Plainion.GraphViz.Model
{
    [Serializable]
    public class Edge : IGraphItem
    {
        public Edge(Node source, Node target)
        {
            Contract.RequiresNotNull(source, nameof(source));
            Contract.RequiresNotNull(target, nameof(target));

            Source = source;
            Target = target;

            Id = CreateId(source.Id, target.Id);
        }

        public string Id { get; private set; }

        public Node Source { get; private set; }
        public Node Target { get; private set; }

        public static string CreateId(string sourceId, string targetId)
        {
            return $"edge-from-{sourceId}-to-{targetId}";
        }
    }
}
