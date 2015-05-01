using System.Linq;
using Plainion.GraphViz.Model;
using Plainion.GraphViz.Presentation;
using Plainion;

namespace Plainion.GraphViz.Algorithms
{
    public class ShowNodeWithIncomings
    {
        private IGraphPresentation myPresentation;

        public ShowNodeWithIncomings( IGraphPresentation presentation )
        {
            Contract.RequiresNotNull( presentation, "presentation" );

            myPresentation = presentation;
        }

        public void Execute( Node node )
        {
            var nodesToShow = myPresentation.Graph.Edges
                .Where( e => e.Target.Id == node.Id )
                .Select( e => e.Source )
                .ToList();

            nodesToShow.Add( node );

            var mask = new NodeMask();
            mask.Set( nodesToShow );

            var caption = myPresentation.GetPropertySetFor<Caption>().Get( node.Id );
            mask.Label = "Incoming of " + caption.DisplayText;

            var module = myPresentation.GetModule<INodeMaskModule>();
            module.Push( mask );
        }
    }
}
