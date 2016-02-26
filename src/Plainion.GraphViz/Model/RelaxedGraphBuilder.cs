using System;
using System.Collections.Generic;
using System.Linq;
using Plainion.GraphViz.Model;

namespace Plainion.GraphViz.Model
{
    [Serializable]
    public class RelaxedGraphBuilder
    {
        private Graph myGraph;

        public RelaxedGraphBuilder()
        {
            myGraph = new Graph();
        }

        public virtual IGraph Graph
        {
            get { return myGraph; }
        }

        public Node TryAddNode( string nodeId )
        {
            var node = new Node( nodeId );

            if( !myGraph.TryAdd( node ) )
            {
                return null;
            }

            return node;
        }

        public Edge TryAddEdge( string sourceNodeId, string targetNodeId )
        {
            var sourceNode = GetOrCreateNode( sourceNodeId );
            var targetNode = GetOrCreateNode( targetNodeId );

            var edge = new Edge( sourceNode, targetNode );

            if( !myGraph.TryAdd( edge ) )
            {
                return null;
            }

            edge.Source.Out.Add( edge );
            edge.Target.In.Add( edge );

            return edge;
        }

        private Node GetOrCreateNode( string nodeId )
        {
            var node = Graph.FindNode( nodeId );
            if( node == null )
            {
                node = new Node( nodeId );
                myGraph.TryAdd( node );
            }

            return node;
        }

        public Cluster TryAddCluster( string clusterId, IEnumerable<string> nodeIds )
        {
            var cluster = new Cluster( clusterId, nodeIds.Select( GetOrCreateNode ) );

            if( !myGraph.TryAdd( cluster ) )
            {
                return null;
            }

            return cluster;
        }
    }
}
