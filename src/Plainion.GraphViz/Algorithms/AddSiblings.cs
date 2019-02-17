using System.Linq;
using Plainion.GraphViz.Model;
using Plainion.GraphViz.Presentation;

namespace Plainion.GraphViz.Algorithms
{
    public class AddSiblings : AbstractAlgorithm
    {
        public AddSiblings(IGraphPresentation presentation)
            : base(presentation)
        {
        }

        public INodeMask Compute(Node node)
        {
            var siblings = node.In.Select(e => e.Source)
                .Concat(node.Out.Select(e => e.Target))
                .Where(n => !Presentation.Picking.Pick(n));

            var mask = new NodeMask();
            mask.Set(siblings);

            var caption = Presentation.GetPropertySetFor<Caption>().Get(node.Id);
            mask.Label = "Siblings of " + caption.DisplayText;

            return mask;
        }
    }
}
