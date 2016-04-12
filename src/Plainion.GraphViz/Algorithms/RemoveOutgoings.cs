using System.Linq;
using Plainion.GraphViz.Model;
using Plainion.GraphViz.Presentation;

namespace Plainion.GraphViz.Algorithms
{
    public class RemoveOutgoings
    {
        private readonly IGraphPresentation myPresentation;

        public RemoveOutgoings(IGraphPresentation presentation)
        {
            Contract.RequiresNotNull(presentation, "presentation");

            myPresentation = presentation;
        }

        public void Execute(Node node)
        {
            var transformationModule = myPresentation.GetModule<ITransformationModule>();
            var nodesToHide = transformationModule.Graph.Edges
                .Where(e => e.Source.Id == node.Id)
                .Where(e => myPresentation.Picking.Pick(e.Target))
                .Select(e => e.Target)
                .ToList();

            var mask = new NodeMask();
            mask.IsShowMask = false;
            mask.Set(nodesToHide);

            var caption = myPresentation.GetPropertySetFor<Caption>().Get(node.Id);
            mask.Label = "(-) outgoings of " + caption.DisplayText;

            var module = myPresentation.GetModule<INodeMaskModule>();
            module.Push(mask);
        }
    }
}
