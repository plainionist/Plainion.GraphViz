using System.Collections.Generic;
using System.Linq;
using Plainion.GraphViz.Model;
using Plainion.GraphViz.Presentation;

namespace Plainion.GraphViz.Algorithms
{
    public class ShowSiblings
    {
        private IGraphPresentation myPresentation;

        public ShowSiblings(IGraphPresentation presentation)
        {
            Contract.RequiresNotNull(presentation, nameof(presentation));

            myPresentation = presentation;
        }

        public void Execute(Node node)
        {
            var nodesToShow = GetNodeWithSiblings(node);

            var mask = new NodeMask();
            mask.Set(nodesToShow);

            var caption = myPresentation.GetPropertySetFor<Caption>().Get(node.Id);
            mask.Label = "Siblings of " + caption.DisplayText;

            var module = myPresentation.GetModule<INodeMaskModule>();
            module.Push(mask);
        }

        private IEnumerable<Node> GetNodeWithSiblings(Node node)
        {
            var transformationModule = myPresentation.GetModule<ITransformationModule>();
            foreach (var edge in transformationModule.Graph.Edges)
            {
                if (edge.Source.Id == node.Id || edge.Target.Id == node.Id)
                {
                    yield return edge.Source;
                    yield return edge.Target;
                }
            }
        }

        public void Select(Node node)
        {
            var nodes = GetNodeWithSiblings(node).ToList();
            nodes.Add(node);

            var selection = myPresentation.GetPropertySetFor<Selection>();
            foreach (var e in node.In)
            {
                selection.Get(e.Id).IsSelected = true;
            }
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
