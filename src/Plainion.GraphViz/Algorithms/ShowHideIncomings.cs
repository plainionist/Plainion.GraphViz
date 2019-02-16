using System.Collections.Generic;
using System.Linq;
using Plainion.GraphViz.Model;
using Plainion.GraphViz.Presentation;

namespace Plainion.GraphViz.Algorithms
{
    public class ShowHideIncomings
    {
        private readonly IGraphPresentation myPresentation;

        public ShowHideIncomings(IGraphPresentation presentation)
        {
            Contract.RequiresNotNull(presentation, nameof(presentation));

            myPresentation = presentation;
        }

        public void Execute(Node node, bool show)
        {
            var nodesToShow = GetIncomings(node);
            if (show)
            {
                nodesToShow.Add(node);
            }

            var mask = new NodeMask();
            mask.IsShowMask = show;

            mask.Set(nodesToShow);

            var caption = myPresentation.GetPropertySetFor<Caption>().Get(node.Id);
            mask.Label = "Incoming of " + caption.DisplayText;

            var module = myPresentation.GetModule<INodeMaskModule>();
            module.Push(mask);
        }

        private IList<Node> GetIncomings(Node node)
        {
            var transformationModule = myPresentation.GetModule<ITransformationModule>();
            return transformationModule.Graph.Edges
                .Where(e => e.Target.Id == node.Id)
                .Select(e => e.Source)
                .ToList();
        }

        public void Select(Node node)
        {
            var nodes = GetIncomings(node);
            nodes.Add(node);

            var selection = myPresentation.GetPropertySetFor<Selection>();
            foreach (var e in node.In)
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
