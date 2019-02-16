using System.Linq;
using Plainion.GraphViz.Presentation;

namespace Plainion.GraphViz.Algorithms
{
    public class ShowNodesOutsideClusters
    {
        private readonly IGraphPresentation myPresentation;

        public ShowNodesOutsideClusters( IGraphPresentation presentation )
        {
            Contract.RequiresNotNull(presentation, nameof(presentation));

            myPresentation = presentation;
        }

        public void Execute()
        {
            var transformationModule = myPresentation.GetModule<ITransformationModule>();
            var nodesToHide = transformationModule.Graph.Nodes
                .Where( node => !( transformationModule.Graph.Clusters.Any( c => c.Nodes.Any( n => n.Id == node.Id ) ) ) );

            var mask = new NodeMask();
            mask.IsShowMask = true;
            mask.Set( nodesToHide );
            mask.Label = "Nodes outside clusters";

            var module = myPresentation.GetModule<INodeMaskModule>();
            module.Push( mask );
        }
    }
}
