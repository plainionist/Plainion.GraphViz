using System.Linq;
using Plainion.GraphViz.Model;
using Plainion.GraphViz.Presentation;

namespace Plainion.GraphViz.Algorithms
{
    public class ShowNodeWithOutgoings
    {
        private readonly IGraphPresentation myPresentation;

        public ShowNodeWithOutgoings( IGraphPresentation presentation )
        {
            Contract.RequiresNotNull( presentation, "presentation" );

            myPresentation = presentation;
        }

        public void Execute( Node node )
        {
            var transformationModule = myPresentation.GetModule<ITransformationModule>();
            var nodesToShow = transformationModule.Graph.Edges
                .Where( e => e.Source.Id == node.Id )
                .Select( e => e.Target )
                .ToList();

            nodesToShow.Add( node );
            var mask = new NodeMask();
            mask.Set( nodesToShow );

            var caption = myPresentation.GetPropertySetFor<Caption>().Get( node.Id );
            mask.Label = "Outgoing of " + caption.DisplayText;

            var module = myPresentation.GetModule<INodeMaskModule>();
            module.Push( mask );
        }
    }
}
