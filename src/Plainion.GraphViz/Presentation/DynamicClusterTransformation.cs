using System.Collections.Generic;
using System.Linq;
using Plainion.GraphViz.Model;

namespace Plainion.GraphViz.Presentation
{
    public class DynamicClusterTransformation : IGraphTransformation
    {
        private List<string> myNodes;

        public DynamicClusterTransformation( string clusterId, string nodeId )
        {
            ClusterId = clusterId;
            myNodes = new List<string> { nodeId };
        }

        public string ClusterId { get; set; }

        public void Add( string nodeId )
        {
            myNodes.Add( nodeId );
        }

        public IGraph Transform( IGraph graph )
        {
            var result = new Graph();

            foreach( var node in graph.Nodes )
            {
                result.Add( node );
            }

            foreach( var edge in graph.Edges )
            {
                result.Add( edge );
            }

            foreach( var cluster in graph.Clusters )
            {
                if( cluster.Id == ClusterId )
                {
                    var nodes = cluster.Nodes
                        .Concat(graph.Nodes.Where(n=>myNodes.Contains(n.Id)));
                    var newCluster = new Cluster( ClusterId, nodes );
                    result.Add( newCluster );
                }
                else
                {
                    result.Add( cluster );
                }
            }

            return result;
        }
    }
}
