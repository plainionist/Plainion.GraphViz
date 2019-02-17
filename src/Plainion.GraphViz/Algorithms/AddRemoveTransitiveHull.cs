using System.Collections.Generic;
using System.Linq;
using Plainion.GraphViz.Model;
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
            Show = true;
            Reverse = false;
        }

        /// <summary>
        /// True: generates a mask containing the given nodes and their transitive hull.
        /// False: generates a mask containing all nodes but the given nodes and their transitive hull.
        /// Default: true.
        /// </summary>
        public bool Show { get; set; }

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
            mask.IsShowMask = Show;

            if (Show)
            {
                mask.Set(connectedNodes);
            }
            else
            {
                var transformationModule = Presentation.GetModule<ITransformationModule>();
                mask.Set(transformationModule.Graph.Nodes.Where(Presentation.Picking.Pick).Except(connectedNodes));
            }

            if (nodes.Count == 1)
            {
                var caption = Presentation.GetPropertySetFor<Caption>().Get(nodes.First().Id);
                mask.Label = (Reverse ? "Reverse t" : "T") + "ransitive hull of " + caption.DisplayText;
            }
            else
            {
                mask.Label = (Reverse ? "Reverse t" : "T") + "ransitive hull of multiple nodes";
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
            if (Show)
            {
                return Reverse ? n.In : n.Out;
            }
            else
            {
                return Reverse ? n.In.Where(e => Presentation.Picking.Pick(e.Source)) : n.Out.Where(e => Presentation.Picking.Pick(e.Target));
            }
        }
    }
}
