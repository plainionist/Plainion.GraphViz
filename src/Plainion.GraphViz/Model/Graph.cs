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
        private IDictionary<string, Cluster> myClusters;

        public Graph()
        {
            myNodes = new Dictionary<string, Node>();
            myEdges = new Dictionary<string, Edge>();
            myClusters = new Dictionary<string, Cluster>();
        }

        public IEnumerable<Node> Nodes { get { return myNodes.Values; } }
        public IEnumerable<Edge> Edges { get { return myEdges.Values; } }
        public IEnumerable<Cluster> Clusters { get { return myClusters.Values; } }

        public bool TryAdd( Node node )
        {
            Contract.RequiresNotNull( node, "node" );

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
            Contract.RequiresNotNull( edge, "edge" );
            
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

        public bool TryAdd( Cluster cluster )
        {
            Contract.RequiresNotNull( cluster, "cluster" );
            
            if( myClusters.ContainsKey( cluster.Id ) )
            {
                return false;
            }

            myClusters.Add( cluster.Id, cluster );

            return true;
        }

        public void Add( Cluster cluster )
        {
            if( !TryAdd( cluster ) )
            {
                throw new ArgumentException( "Cluster already exists: " + cluster.Id );
            }
        }

        public Node FindNode(string nodeId)
        {
            Node node;
            if (myNodes.TryGetValue(nodeId, out node))
            {
                return node;
            }

            return null;
        }

        public Node GetNode(string nodeId)
        {
            var node = FindNode(nodeId);
            Contract.Requires(node != null, "Node not found: " + nodeId);
            return node;
        }
    }
}
