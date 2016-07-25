using System.Collections.Generic;
using System.Linq;
using Plainion.GraphViz.Model;
using Plainion.GraphViz.Presentation;

namespace Plainion.GraphViz.Algorithms
{
    public class HideNodes
    {
        private readonly IGraphPresentation myPresentation;

        public HideNodes(IGraphPresentation presentation)
        {
            Contract.RequiresNotNull(presentation, "presentation");

            myPresentation = presentation;
        }

        public void Execute(params Node[] nodes)
        {
            Execute((IEnumerable<Node>)nodes);
        }

        public void Execute(IEnumerable<Node> nodes)
        {
            var mask = new NodeMask();
            mask.IsShowMask = false;
            mask.Set(nodes);

            if (nodes.Count() == 1)
            {
                var caption = myPresentation.GetPropertySetFor<Caption>().Get(nodes.First().Id);
                mask.Label = caption.DisplayText;
            }
            else
            {
                var caption = myPresentation.GetPropertySetFor<Caption>().Get(nodes.First().Id);
                mask.Label = caption.DisplayText + "...";
            }

            var module = myPresentation.GetModule<INodeMaskModule>();
            module.Push(mask);
        }
    }
}
