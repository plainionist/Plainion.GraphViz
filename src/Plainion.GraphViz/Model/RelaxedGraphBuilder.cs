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
            var sourceNode = Graph.FindNode( sourceNodeId );
            if( sourceNode == null )
            {
                // support just adding edges - add nodes implicitly
                sourceNode = new Node( sourceNodeId );
                myGraph.TryAdd( sourceNode );
            }

            var targetNode = Graph.FindNode( targetNodeId );
            if( targetNode == null )
            {
                // support just adding edges - add nodes implicitly
                targetNode = new Node( targetNodeId );
                myGraph.TryAdd( targetNode );
            }

            var edge = new Edge( sourceNode, targetNode );

            if( !myGraph.TryAdd( edge ) )
            {
                return null;
            }

            edge.Source.Out.Add( edge );
            edge.Target.In.Add( edge );

            return edge;
        }

        public Cluster TryAddCluster( string clusterId, IEnumerable<string> nodeIds )
        {
            var cluster = new Cluster( clusterId, nodeIds.Select( n => TryAddNode( n ) ) );

            if( !myGraph.TryAdd( cluster ) )
            {
                return null;
            }

            return cluster;
        }
    }
}
