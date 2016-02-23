using System.Linq;
using Plainion.GraphViz.Model;
using Plainion.GraphViz.Presentation;

namespace Plainion.GraphViz.Algorithms
{
    public class HideSingleNode
    {
        private IGraphPresentation myPresentation;

        public HideSingleNode( IGraphPresentation presentation )
        {
            Contract.RequiresNotNull( presentation, "presentation" );

            myPresentation = presentation;
        }

        public void Execute( Node node )
        {
            var nodesToHide = myPresentation.Graph.Nodes
                .Where( n => n.Id == node.Id );

            var mask = new NodeMask();
            mask.IsShowMask = false;
            mask.Set( nodesToHide );

            var caption = myPresentation.GetPropertySetFor<Caption>().Get( node.Id );
            mask.Label = caption.DisplayText;

            var module = myPresentation.GetModule<INodeMaskModule>();
            module.Push( mask );
        }
    }
}
