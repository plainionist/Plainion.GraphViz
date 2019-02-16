using System;
using System.Collections.Generic;
using System.Linq;
using Plainion.GraphViz.Model;
using Plainion.GraphViz.Presentation;

namespace Plainion.GraphViz.Algorithms
{
    public class ShowHideNodes
    {
        private readonly IGraphPresentation myPresentation;
        private bool myShow;
        private bool myNegate;

        public ShowHideNodes(IGraphPresentation presentation, bool show)
            : this(presentation, show, false)
        {
        }

        public ShowHideNodes(IGraphPresentation presentation, bool show, bool negate)
        {
            Contract.RequiresNotNull(presentation, nameof(presentation));

            myPresentation = presentation;
            myShow = show;
            myNegate = negate;
        }

        public void Execute(params Node[] nodes)
        {
            Execute((IEnumerable<Node>)nodes);
        }

        public void Execute(IEnumerable<Node> nodes)
        {
            var mask = new NodeMask();
            mask.IsShowMask = myShow;

            if (myNegate)
            {
                var transformationModule = myPresentation.GetModule<ITransformationModule>();
                mask.Set(transformationModule.Graph.Nodes.Except(nodes));
            }
            else
            {
                mask.Set(nodes);
            }

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

            if (myNegate)
            {
                mask.Label = "Not " + mask.Label;
            }

            var module = myPresentation.GetModule<INodeMaskModule>();
            module.Push(mask);
        }
    }
}
