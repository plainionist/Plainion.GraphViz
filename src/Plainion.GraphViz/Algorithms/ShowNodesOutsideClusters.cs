using System.Linq;
using Plainion.GraphViz.Model;
using Plainion.GraphViz.Presentation;

namespace Plainion.GraphViz.Algorithms
{
    public class ShowNodesOutsideClusters
    {
        private readonly IGraphPresentation myPresentation;

        public ShowNodesOutsideClusters( IGraphPresentation presentation )
        {
            Contract.RequiresNotNull( presentation, "presentation" );

            myPresentation = presentation;
        }

        public void Execute()
        {
            var nodesToHide = myPresentation.Graph.Nodes
                .Where( n => !IsInAnyCluster( n ) );

            var mask = new NodeMask();
            mask.IsShowMask = true;
            mask.Set( nodesToHide );
            mask.Label = "Nodes outside clusters";

            var module = myPresentation.GetModule<INodeMaskModule>();
            module.Push( mask );
        }

        private bool IsInAnyCluster( Node node )
        {
            return myPresentation.Graph.Clusters.Any( c => c.Nodes.Any( n => n.Id == node.Id ) );
        }
    }
}
