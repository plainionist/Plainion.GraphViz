using System.Collections.Generic;
using System.Linq;
using Plainion.GraphViz.Model;
using Plainion.GraphViz.Presentation;

namespace Plainion.GraphViz.Algorithms
{
    public class RemoveNotConnectedNodes
    {
        private readonly IGraphPresentation myPresentation;

        public RemoveNotConnectedNodes(IGraphPresentation presentation)
        {
            Contract.RequiresNotNull(presentation, "presentation");

            myPresentation = presentation;
        }

        public void Execute(Node node)
        {
            var connectedNodes = GetConnectedNodes(node);

            var transformationModule = myPresentation.GetModule<ITransformationModule>();
            var nodesToHide = transformationModule.Graph.Nodes
                .Except(connectedNodes);

            var mask = new NodeMask();
            mask.IsShowMask = false;
            mask.Set(nodesToHide);

            var caption = myPresentation.GetPropertySetFor<Caption>().Get(node.Id);
            mask.Label = "(-) non-connected's of " + caption.DisplayText;

            var module = myPresentation.GetModule<INodeMaskModule>();
            module.Push(mask);
        }

        private IEnumerable<Node> GetConnectedNodes(Node node)
        {
            var connectedNodes = new HashSet<Node>();
            connectedNodes.Add(node);

            var recursiveSiblings = Traverse.BreathFirst(new[] { node },
                    n => n.Out.Where(e => myPresentation.Picking.Pick(e.Target))
                        .Concat(n.In.Where(e => myPresentation.Picking.Pick(e.Source))))
                .SelectMany(e => new[] { e.Source, e.Target });

            foreach (var n in recursiveSiblings)
            {
                connectedNodes.Add(n);
            }

            return connectedNodes;
        }
    }
}
