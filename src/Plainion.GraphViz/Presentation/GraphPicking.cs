using System.Linq;
using Plainion.GraphViz.Model;

namespace Plainion.GraphViz.Presentation
{
    internal class GraphPicking : IGraphPicking
    {
        private IGraphPresentation myPresentation;

        public GraphPicking(IGraphPresentation presentation)
        {
            myPresentation = presentation;
        }

        public bool Pick(Node node)
        {
            var masks = myPresentation.GetModule<INodeMaskModule>().Items
                .Where(s => s.IsApplied);

            foreach (var mask in masks)
            {
                var hitValue = mask.IsSet(node);
                if (hitValue == null)
                {
                    // this mask contains no information about the given node
                    continue;
                }

                return hitValue.Value;
            }

            // by default - if nothing else is calculated based on the masks - all nodes are visible
            return true;
        }

        public bool Pick(Edge edge)
        {
            return Pick(edge.Source) && Pick(edge.Target);
        }
    }
}
