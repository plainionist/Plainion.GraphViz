using System;
using System.Collections.Generic;
using System.Linq;
using Plainion.GraphViz.Model;
using Plainion.GraphViz.Presentation;

namespace Plainion.GraphViz.Algorithms
{
    public class TracePath
    {
        private readonly IGraphPresentation myPresentation;

        public TracePath(IGraphPresentation presentation)
        {
            Contract.RequiresNotNull(presentation, "presentation");

            myPresentation = presentation;
        }

        public void Execute(Node from, Node to)
        {
            var captions = myPresentation.GetModule<ICaptionModule>();

            var mask = new NodeMask();
            mask.Label = string.Format("Path from {0} to {1}", captions.Get(from.Id), captions.Get(to.Id));
            mask.IsShowMask = true;
            mask.Set(GetPaths(from, to));

            var module = myPresentation.GetModule<INodeMaskModule>();
            module.Push(mask);
        }

        private IEnumerable<Node> GetPaths(Node source, Node target)
        {
            var reachableFromSource = GetReachableNodes(source, n => n.Out.Where(e => myPresentation.Picking.Pick(e.Target)));
            var reachingTarget = GetReachableNodes(target, n => n.In.Where(e => myPresentation.Picking.Pick(e.Source)));

            return reachableFromSource
                .Intersect(reachingTarget)
                .ToList();
        }

        private IEnumerable<Node> GetReachableNodes(Node node, Func<Node, IEnumerable<Edge>> selector)
        {
            var connectedNodes = new HashSet<Node>();
            connectedNodes.Add(node);

            var recursiveSiblings = Traverse.BreathFirst(new[] { node }, selector)
                .SelectMany(e => new[] { e.Source, e.Target });

            foreach (var n in recursiveSiblings)
            {
                connectedNodes.Add(n);
            }

            return connectedNodes;
        }
    }
}
