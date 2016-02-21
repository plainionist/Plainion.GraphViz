using System;
using System.Collections.Generic;

namespace Plainion.GraphViz.Model
{
    [Serializable]
    public class Node : IGraphItem
    {
        public Node( string id )
        {
            Id = id;

            In = new List<Edge>();
            Out = new List<Edge>();
        }

        public string Id { get; private set; }

        public IList<Edge> In { get; private set; }
        public IList<Edge> Out { get; private set; }
    }
}
