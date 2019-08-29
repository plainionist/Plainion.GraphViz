using System.Linq;
using System.Xml.Linq;

namespace Plainion.GraphViz.Modules.Documents
{
    class GraphMLDocument : AbstractGraphDocument
    {
        protected override void Load()
        {
            var root = XElement.Load( Filename ).Elements( XN( "graph" ) ).First();

            foreach( var xmlNode in root.Elements( XN( "node" ) ) )
            {
                TryAddNode( xmlNode.Attribute( "id" ).Value );
            }

            foreach( var xmlLink in root.Elements( XN( "edge" ) ) )
            {
                TryAddEdge( xmlLink.Attribute( "source" ).Value, xmlLink.Attribute( "target" ).Value );
            }
        }

        private static XName XN( string localName )
        {
            return XName.Get( localName, "http://graphml.graphdrawing.org/xmlns" );
        }
    }
}
