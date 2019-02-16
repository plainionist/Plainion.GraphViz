using System.Collections.Generic;
using System.Linq;
using Plainion.GraphViz.Model;
using Plainion.GraphViz.Presentation;

namespace Plainion.GraphViz.Algorithms
{
    public class ShowHideOutgoings
    {
        private readonly IGraphPresentation myPresentation;

        public ShowHideOutgoings( IGraphPresentation presentation )
        {
            Contract.RequiresNotNull(presentation, nameof(presentation));

            myPresentation = presentation;
        }

        public void Execute( Node node, bool show)
        {
            var nodesToShow = GetOutgoings(node);
            if (show)
            {
                nodesToShow.Add(node);
            }

            var mask = new NodeMask();
            mask.IsShowMask = show;
            
            mask.Set(nodesToShow);

            var caption = myPresentation.GetPropertySetFor<Caption>().Get( node.Id );
            mask.Label = "Outgoing of " + caption.DisplayText;

            var module = myPresentation.GetModule<INodeMaskModule>();
            module.Push( mask );
        }

        private IList<Node> GetOutgoings(Node node)
        {
            var transformationModule = myPresentation.GetModule<ITransformationModule>();
            return transformationModule.Graph.Edges
                .Where(e => e.Source.Id == node.Id)
                .Select(e => e.Target)
                .ToList();
        }

        public void Select(Node node)
        {
            var nodes = GetOutgoings(node);
            nodes.Add(node);

            var selection = myPresentation.GetPropertySetFor<Selection>();
            foreach (var e in node.Out)
            {
                selection.Get(e.Id).IsSelected = true;
            }
            foreach (var n in nodes)
            {
                selection.Get(n.Id).IsSelected = true;
            }
        }
    }
}
