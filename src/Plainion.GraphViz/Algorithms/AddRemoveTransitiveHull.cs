using System.Collections.Generic;
using System.Linq;
using Plainion.Graphs;
using Plainion.GraphViz.Presentation;

namespace Plainion.GraphViz.Algorithms
{
    /// <summary>
    /// Describes all nodes directly and indirectly connected to the given one
    /// </summary>
    public class AddRemoveTransitiveHull : AbstractAlgorithm
    {
        public AddRemoveTransitiveHull(IGraphPresentation presentation)
            : base(presentation)
        {
            Add = true;
            Reverse = false;
        }

        /// <summary>
        /// True: generates a mask containing the given nodes and their transitive hull.
        /// False: generates a mask containing all nodes but the given nodes and their transitive hull.
        /// Default: true.
        /// </summary>
        public bool Add { get; set; }

        /// <summary>
        /// True: considers only "in" edges when building the hull.
        /// False: considers only "out" edges when building the hull.
        /// Default: false.
        /// </summary>
        public bool Reverse { get; set; }

        public INodeMask Compute(IReadOnlyCollection<Node> nodes)
        {
            var connectedNodes = nodes
                .SelectMany(n => GetReachableNodes(n))
                .Distinct();

            var mask = new NodeMask();
            mask.IsShowMask = Add;

            mask.Set(connectedNodes);

            if (nodes.Count == 0)
            {
                mask.Label = "<empty>";
            }
            else if (nodes.Count == 1)
            {
                var caption = Presentation.GetPropertySetFor<Caption>().Get(nodes.First().Id);
                mask.Label = (Reverse ? "Nodes reaching " : "Reachable nodes of ") + caption.DisplayText;
            }
            else
            {
                var caption = Presentation.GetPropertySetFor<Caption>().Get(nodes.First().Id);
                mask.Label = (Reverse ? "Nodes reaching " : "Reachable nodes of ") + caption.DisplayText + " and ...";
            }

            return mask;
        }

        private IEnumerable<Node> GetReachableNodes(Node node)
        {
            var connectedNodes = new HashSet<Node>();
            connectedNodes.Add(node);

            var recursiveSiblings = Traverse.BreathFirst(new[] { node }, n => SelectSiblings(n))
                .SelectMany(e => new[] { e.Source, e.Target });

            foreach (var n in recursiveSiblings)
            {
                connectedNodes.Add(n);
            }

            return connectedNodes;
        }

        private IEnumerable<Edge> SelectSiblings(Node n)
        {
            if (Add)
            {
                return Reverse ? n.In.Where(e => !Presentation.Picking.Pick(e.Source)) : n.Out.Where(e => !Presentation.Picking.Pick(e.Target));
            }
            else
            {
                return Reverse ? n.In.Where(e => Presentation.Picking.Pick(e.Source)) : n.Out.Where(e => Presentation.Picking.Pick(e.Target));
            }
        }
    }
}
