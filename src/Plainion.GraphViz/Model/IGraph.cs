using System.Collections.Generic;

namespace Plainion.GraphViz.Model
{
    public interface IGraph
    {
        IEnumerable<Node> Nodes { get; }
        IEnumerable<Edge> Edges { get; }

        Node FindNode( string nodeId );
    }
}
