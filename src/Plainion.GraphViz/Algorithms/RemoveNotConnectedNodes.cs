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

            var newNodes = new HashSet<Node>(connectedNodes);

            while (newNodes.Count > 0)
            {
                var nodes = newNodes.ToList();
                newNodes.Clear();

                foreach (var newNode in nodes)
                {
                    var siblings = newNode.In
                        .Select(e => e.Source)
                        .Concat(node.Out
                            .Select(e => e.Target))
                        .Where(n => myPresentation.Picking.Pick(n));

                    foreach (var sibling in siblings)
                    {
                        connectedNodes.Add(sibling);
                        newNodes.Add(sibling);
                    }
                }

                newNodes.ExceptWith(connectedNodes);
            }

            return connectedNodes;
        }
    }
}
