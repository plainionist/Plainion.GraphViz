using System;
using System.Collections.Generic;
using System.Linq;

namespace Plainion.GraphViz.Model
{
    [Serializable]
    public class Cluster : AbstractGraphItem
    {
        public Cluster(string id, IEnumerable<Node> nodes)
            :base(id)
        {
            Nodes = nodes.ToList();
        }

        public IEnumerable<Node> Nodes { get; private set; }
    }
}
