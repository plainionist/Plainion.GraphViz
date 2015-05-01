using System.Collections.Generic;
using Plainion.GraphViz.Model;

namespace Plainion.GraphViz.Presentation
{
    public interface IGraphLayoutModule : IModule<AbstractPropertySet>
    {
        void Add( NodeLayout layout );
        void Add( EdgeLayout layout );
        void Clear();

        NodeLayout GetLayout( Node node );
        EdgeLayout GetLayout( Edge edge );

        void Set( IEnumerable<NodeLayout> nodeLayouts, IEnumerable<EdgeLayout> edgeLayouts );
    }
}
