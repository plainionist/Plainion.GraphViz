using System.Linq;
using Plainion.GraphViz.Presentation;

namespace Plainion.GraphViz.Algorithms
{
    public class AddVisibleNodesOutsideClustersToCluster
    {
        private readonly IGraphPresentation myPresentation;

        public AddVisibleNodesOutsideClustersToCluster(IGraphPresentation presentation)
        {
            Contract.RequiresNotNull(presentation, "presentation");

            myPresentation = presentation;
        }

        public void Execute(string clusterId)
        {
            var transformationModule = myPresentation.GetModule<ITransformationModule>();

            var nodes = transformationModule.Graph.Nodes
                .Where(node => myPresentation.Picking.Pick(node))
                .Where(node => !transformationModule.Graph.Clusters.Any(c => c.Nodes.Any(n => n.Id == node.Id)))
                .Select(node => node.Id)
                .ToList();

            new ChangeClusterAssignment(myPresentation)
                .Execute(t => t.AddToCluster(nodes, clusterId));
        }
    }
}
