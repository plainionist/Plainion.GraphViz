using System;
using Plainion;

namespace Plainion.GraphViz.Model
{
    [Serializable]
    public class Edge : IGraphItem
    {
        public Edge( Node source, Node target )
        {
            Contract.RequiresNotNull( source, "source" );
            Contract.RequiresNotNull( target, "target" );

            Source = source;
            Target = target;

            Id = CreateId( Source.Id, Target.Id );
        }

        public string Id { get; private set; }

        public Node Source { get; private set; }
        public Node Target { get; private set; }

        public static string CreateId( string sourceNodeId, string targetNodeId )
        {
            return string.Format( "edge-from-{0}-to-{1}", sourceNodeId, targetNodeId );
        }
    }
}
