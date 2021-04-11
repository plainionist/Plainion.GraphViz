using System;
using System.Collections.Generic;
using System.Linq;

namespace Plainion.GraphViz.Model
{
    [Serializable]
    public class Cluster : IGraphItem
    {
        public Cluster(string id, IEnumerable<Node> nodes)
        {
            Contract.RequiresNotNullNotEmpty(id, nameof(id));

            Id = id;

            Nodes = nodes.ToList();
        }

        public string Id { get; }

        public IReadOnlyCollection<Node> Nodes { get; }
    }
}
