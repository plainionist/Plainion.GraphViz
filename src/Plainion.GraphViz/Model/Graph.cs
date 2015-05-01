using System;
using System.Collections.Generic;
using System.Linq;

namespace Plainion.GraphViz.Model
{
    [Serializable]
    public class Graph : IGraph
    {
        private IDictionary<string, Node> myNodes;
        private IDictionary<string, Edge> myEdges;

        public Graph()
        {
            myNodes = new Dictionary<string, Node>();
            myEdges = new Dictionary<string, Edge>();
        }

        public IEnumerable<Node> Nodes { get { return myNodes.Values; } }
        public IEnumerable<Edge> Edges { get { return myEdges.Values; } }

        public bool TryAdd( Node node )
        {
            if( myNodes.ContainsKey( node.Id ) )
            {
                return false;
            }

            myNodes.Add( node.Id, node );

            return true;
        }

        public void Add( Node node )
        {
            if( !TryAdd( node ) )
            {
                throw new ArgumentException( "Node already exists: " + node.Id );
            }
        }

        public bool TryAdd( Edge edge )
        {
            if( myEdges.ContainsKey( edge.Id ) )
            {
                return false;
            }

            myEdges.Add( edge.Id, edge );

            return true;
        }

        public void Add( Edge edge )
        {
            if( !TryAdd( edge ) )
            {
                throw new ArgumentException( "Edge already exists: " + edge.Id );
            }
        }

        public Node FindNode( string nodeId )
        {
            Node node;
            if( myNodes.TryGetValue( nodeId, out node ) )
            {
                return node;
            }

            return null;
        }
    }
}
