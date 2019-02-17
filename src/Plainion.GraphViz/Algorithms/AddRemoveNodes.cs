using System.Collections.Generic;
using System.Linq;
using Plainion.GraphViz.Model;
using Plainion.GraphViz.Presentation;

namespace Plainion.GraphViz.Algorithms
{
    /// <summary>
    /// Generates show/hide mask of all nodes given.
    /// In case of "negation" all visible nodes except the given ones are considered
    /// </summary>
    public class AddRemoveNodes : AbstractAlgorithm
    {
        private bool myShow;
        private bool myNegate;

        public AddRemoveNodes(IGraphPresentation presentation, bool show)
            : this(presentation, show, false)
        {
        }

        public AddRemoveNodes(IGraphPresentation presentation, bool show, bool negate)
            : base(presentation)
        {
            myShow = show;
            myNegate = negate;
        }

        public INodeMask Compute(params Node[] nodes)
        {
            return Compute((IEnumerable<Node>)nodes);
        }

        public INodeMask Compute(IEnumerable<Node> nodes)
        {
            var mask = new NodeMask();
            mask.IsShowMask = myShow;

            if (myNegate)
            {
                var result = Presentation.GetModule<ITransformationModule>().Graph.Nodes
                    .Where(Presentation.Picking.Pick)
                    .Except(nodes);
                mask.Set(result);
            }
            else
            {
                mask.Set(nodes);
            }

            if (nodes.Count() == 1)
            {
                var caption = Presentation.GetPropertySetFor<Caption>().Get(nodes.First().Id);
                mask.Label = caption.DisplayText;
            }
            else
            {
                var caption = Presentation.GetPropertySetFor<Caption>().Get(nodes.First().Id);
                mask.Label = caption.DisplayText + "...";
            }

            if (myNegate)
            {
                mask.Label = "Not " + mask.Label;
            }

            return mask;
        }
    }
}
