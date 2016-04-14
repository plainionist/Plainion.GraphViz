using System.Linq;
using Plainion.GraphViz.Model;
using Plainion.GraphViz.Presentation;

namespace Plainion.GraphViz.Algorithms
{
    public class ShowHideIncomings
    {
        private readonly IGraphPresentation myPresentation;

        public ShowHideIncomings( IGraphPresentation presentation )
        {
            Contract.RequiresNotNull( presentation, "presentation" );

            myPresentation = presentation;
        }

        public void Execute( Node node, bool show )
        {
            var transformationModule = myPresentation.GetModule<ITransformationModule>();
            var nodesToShow = transformationModule.Graph.Edges
                .Where( e => e.Target.Id == node.Id )
                .Select( e => e.Source )
                .ToList();

            if (show)
            {
                nodesToShow.Add(node);
            }

            var mask = new NodeMask();
            mask.IsShowMask = show;

            mask.Set(nodesToShow);

            var caption = myPresentation.GetPropertySetFor<Caption>().Get( node.Id );
            mask.Label = "Incoming of " + caption.DisplayText;

            var module = myPresentation.GetModule<INodeMaskModule>();
            module.Push( mask );
        }
    }
}
