using System.Linq;
using Plainion.GraphViz.Model;
using Plainion.GraphViz.Presentation;

namespace Plainion.GraphViz.Algorithms
{
    /// <summary>
    /// Generates show/hide mask of all target nodes
    /// </summary>
    public class AddRemoveTargets : AbstractAlgorithm
    {
        public AddRemoveTargets(IGraphPresentation presentation)
            : base(presentation)
        {
        }

        public INodeMask Compute(Node node, bool show)
        {
            var targets = node.Out
                .Select(e => e.Target)
                .Where(n => Presentation.Picking.Pick(n) != show);

            var mask = new NodeMask();
            mask.IsShowMask = show;

            mask.Set(targets);

            var caption = Presentation.GetPropertySetFor<Caption>().Get(node.Id);
            mask.Label = "Outgoing of " + caption.DisplayText;

            return mask;
        }
    }
}
