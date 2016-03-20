using System.Collections.Generic;
using Plainion.GraphViz.Model;
using Plainion.GraphViz.Presentation;
using Plainion;

namespace Plainion.GraphViz.Algorithms
{
    public class ShowNodeWithSiblings
    {
        private IGraphPresentation myPresentation;

        public ShowNodeWithSiblings( IGraphPresentation presentation )
        {
            Contract.RequiresNotNull( presentation, "presentation" );

            myPresentation = presentation;
        }

        public void Execute( Node node )
        {
            var nodesToShow = GetNodeWithSiblings( node.Id );

            var mask = new NodeMask();
            mask.Set( nodesToShow );

            var caption = myPresentation.GetPropertySetFor<Caption>().Get( node.Id );
            mask.Label = "Siblings of " + caption.DisplayText;

            var module = myPresentation.GetModule<INodeMaskModule>();
            module.Push( mask );
        }

        private IEnumerable<Node> GetNodeWithSiblings( string nodeId )
        {
            var transformationModule = myPresentation.GetModule<ITransformationModule>();
            foreach( var edge in transformationModule.Graph.Edges )
            {
                if( edge.Source.Id == nodeId || edge.Target.Id == nodeId )
                {
                    yield return edge.Source;
                    yield return edge.Target;
                }
            }
        }
    }
}
