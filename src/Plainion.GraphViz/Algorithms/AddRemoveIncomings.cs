using System.Linq;
using Plainion.GraphViz.Model;
using Plainion.GraphViz.Presentation;

namespace Plainion.GraphViz.Algorithms
{
    public class AddRemoveIncomings
    {
        private readonly IGraphPresentation myPresentation;

        public AddRemoveIncomings(IGraphPresentation presentation)
        {
            Contract.RequiresNotNull(presentation, "presentation");

            myPresentation = presentation;
        }

        public void Execute(Node node, bool add)
        {
            var transformationModule = myPresentation.GetModule<ITransformationModule>();
            var nodesToHide = transformationModule.Graph.Edges
                .Where(e => e.Target.Id == node.Id)
                // only remove what is visible but for add we need to consider explicitly the non-visible
                .Where(e => add || myPresentation.Picking.Pick(e.Source))
                .Select(e => e.Source)
                .ToList();

            var mask = new NodeMask();
            mask.IsShowMask = add;
            mask.Set(nodesToHide);

            var caption = myPresentation.GetPropertySetFor<Caption>().Get(node.Id);
            mask.Label = "(" + (add ? "+" : "-") + ") incomings of " + caption.DisplayText;

            var module = myPresentation.GetModule<INodeMaskModule>();
            module.Push(mask);
        }
    }
}
