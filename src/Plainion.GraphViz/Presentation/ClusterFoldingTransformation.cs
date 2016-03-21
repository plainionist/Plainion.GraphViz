using System;
using System.Linq;
using Plainion.GraphViz.Model;

namespace Plainion.GraphViz.Presentation
{
    public class ClusterFoldingTransformation : IGraphTransformation
    {
        private string myClusterNodeId;

        public ClusterFoldingTransformation( Cluster cluster, IGraphPresentation presentation )
        {
            Cluster = cluster;

            myClusterNodeId = Guid.NewGuid().ToString();

            // encode cluster id again in caption to ensure that cluster is rendered big enough to include cluster caption
            var captions = presentation.GetPropertySetFor<Caption>();
            captions.Add( new Caption( myClusterNodeId, "[" + captions.Get( Cluster.Id ).DisplayText + "]" ) );
        }

        public Cluster Cluster { get; private set; }

        public IGraph Transform( IGraph graph )
        {
            var builder = new RelaxedGraphBuilder();

            builder.TryAddNode( myClusterNodeId );
            builder.TryAddCluster( Cluster.Id, new[] { myClusterNodeId } );

            foreach( var cluster in graph.Clusters.Where( c => c.Id != Cluster.Id ) )
            {
                builder.TryAddCluster( cluster.Id, cluster.Nodes.Select( n => n.Id ) );
            }

            foreach( var edge in graph.Edges )
            {
                var source = edge.Source.Id;
                var target = edge.Target.Id;

                if( Cluster.Nodes.Any( n => n.Id == source ) )
                {
                    source = myClusterNodeId;
                }

                if( Cluster.Nodes.Any( n => n.Id == target ) )
                {
                    target = myClusterNodeId;
                }

                // ignore self-edges
                if( source != target )
                {
                    builder.TryAddEdge( source, target );
                }
            }

            foreach( var node in graph.Nodes.Select( n => n.Id ).Except( Cluster.Nodes.Select( n => n.Id ) ) )
            {
                builder.TryAddNode( node );
            }

            return builder.Graph;
        }
    }
}
