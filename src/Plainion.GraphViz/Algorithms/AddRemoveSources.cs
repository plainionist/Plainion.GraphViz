using System.Collections.Generic;
using System.Linq;
using Plainion.GraphViz.Model;
using Plainion.GraphViz.Presentation;

namespace Plainion.GraphViz.Algorithms
{
    /// <summary>
    /// Generates show/hide mask of all source nodes
    /// </summary>
    public class AddRemoveSources : AbstractAlgorithm
    {
        public AddRemoveSources(IGraphPresentation presentation)
            : base(presentation)
        {
        }

        public INodeMask Compute(Node node, bool show)
        {
            var sources = node.In
                .Select(e => e.Source)
                .Where(n => Presentation.Picking.Pick(n) != show);

            var mask = new NodeMask();
            mask.IsShowMask = show;

            mask.Set(sources);

            var caption = Presentation.GetPropertySetFor<Caption>().Get(node.Id);
            mask.Label = "Sources of " + caption.DisplayText;

            return mask;
        }
    }
}
