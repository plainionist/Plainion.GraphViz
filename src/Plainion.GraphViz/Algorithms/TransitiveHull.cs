using System;
using System.Collections.Generic;
using System.Linq;
using Plainion.GraphViz.Model;
using Plainion.GraphViz.Presentation;

namespace Plainion.GraphViz.Algorithms
{
    /// <summary>
    /// Describes all nodes directly and indirectly connected to the given one
    /// </summary>
    public class TransitiveHull
    {
        private readonly IGraphPresentation myPresentation;
        private bool myShow;

        public TransitiveHull(IGraphPresentation presentation, bool show)
        {
            Contract.RequiresNotNull(presentation, "presentation");

            myPresentation = presentation;
            myShow = show;
        }

        public void Execute(IReadOnlyCollection<Node> nodes, bool reverse = false)
        {
            var connectedNodes = nodes
                .SelectMany(n => GetReachableNodes(n, reverse))
                .Distinct();

            var mask = new NodeMask();
            mask.IsShowMask = myShow;
            if (myShow)
            {
                mask.Set(connectedNodes);
            }
            else
            {
                var transformationModule = myPresentation.GetModule<ITransformationModule>();
                mask.Set(transformationModule.Graph.Nodes.Except(connectedNodes));
            }

            if (nodes.Count == 1)
            {
                var caption = myPresentation.GetPropertySetFor<Caption>().Get(nodes.First().Id);
                mask.Label = (reverse ? "Reverse t" : "T") + "ransitive hull of " + caption.DisplayText;
            }
            else
            {
                mask.Label = (reverse ? "Reverse t" : "T") + "ransitive hull of multiple nodes";
            }

            var module = myPresentation.GetModule<INodeMaskModule>();
            module.Push(mask);
        }

        private IEnumerable<Node> GetReachableNodes(Node node, bool reverse)
        {
            var connectedNodes = new HashSet<Node>();
            connectedNodes.Add(node);

            var recursiveSiblings = Traverse.BreathFirst(new[] { node }, n => SelectSiblings(n, reverse))
                .SelectMany(e => new[] { e.Source, e.Target });

            foreach (var n in recursiveSiblings)
            {
                connectedNodes.Add(n);
            }

            return connectedNodes;
        }

        private IEnumerable<Edge> SelectSiblings(Node n, bool reverse)
        {
            if (myShow)
            {
                return reverse ? n.In : n.Out;
            }
            else
            {
                return reverse ? n.In.Where(e => myPresentation.Picking.Pick(e.Source)) : n.Out.Where(e => myPresentation.Picking.Pick(e.Target));
            }
        }
    }
}
