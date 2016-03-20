using System.Linq;
using Plainion.GraphViz.Model;
using Plainion.GraphViz.Presentation;

namespace Plainion.GraphViz.Algorithms
{
    public class HideNodesWithoutEdges
    {
        private readonly IGraphPresentation myPresentation;

        public HideNodesWithoutEdges(IGraphPresentation presentation)
        {
            Contract.RequiresNotNull(presentation, "presentation");

            myPresentation = presentation;
        }

        public void Execute()
        {
            var transformationModule = myPresentation.GetModule<ITransformationModule>();

            var nodesToHide = transformationModule.Graph.Nodes
                .Where(n => !HasEdges(n));

            var mask = new NodeMask();
            mask.IsShowMask = false;
            mask.Set(nodesToHide);
            mask.Label = "Nodes without Edges";

            var module = myPresentation.GetModule<INodeMaskModule>();
            module.Push(mask);
        }

        private bool HasEdges(Node node)
        {
            return node.In.Any(e => myPresentation.Picking.Pick(e))
                || node.Out.Any(e => myPresentation.Picking.Pick(e));
        }
    }
}
