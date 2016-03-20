using System;
using Plainion.GraphViz.Model;

namespace Plainion.GraphViz.Presentation
{
    class ClusterFoldingTransformation : IGraphTransformation
    {
        private Cluster myCluster;

        public ClusterFoldingTransformation( Cluster cluster )
        {
            myCluster = cluster;
        }

        public IGraph Transform( IGraph graph )
        {
            return graph;
        }
    }
}
