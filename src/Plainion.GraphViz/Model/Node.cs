using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace Plainion.GraphViz.Model
{
    [Serializable]
    [DebuggerDisplay("{Id}")]
    public class Node : AbstractGraphItem
    {
        public Node(string id)
            : base(id)
        {
            In = new List<Edge>();
            Out = new List<Edge>();
        }

        public IList<Edge> In { get; private set; }
        public IList<Edge> Out { get; private set; }
    }
}
