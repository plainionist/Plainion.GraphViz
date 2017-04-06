using Plainion.GraphViz.Model;
using Plainion.GraphViz.Presentation;
using System.Collections.Generic;
using System.Linq;

namespace Plainion.GraphViz.Algorithms
{
    public class ShowMostIncomings
    {
        private IGraphPresentation myPresentation;

        public ShowMostIncomings(IGraphPresentation presentation)
        {
            Contract.RequiresNotNull(presentation, "presentation");

            myPresentation = presentation;
        }

        public void Execute(int numTop)
        {
            var transformationModule = myPresentation.GetModule<ITransformationModule>();
            var nodes = new HashSet<string>(transformationModule.Graph.Nodes
                .OrderByDescending(n => n.In.Count)
                .Take(numTop)
                .Select(n => n.Id));

            var nodesToShow = GetNodesWithSiblings(nodes);

            var mask = new NodeMask();
            mask.Set(nodesToShow);

            mask.Label = "Top " + numTop + " most incomings";

            var module = myPresentation.GetModule<INodeMaskModule>();
            module.Push(mask);
        }

        private IEnumerable<Node> GetNodesWithSiblings(HashSet<string> nodeIds)
        {
            var transformationModule = myPresentation.GetModule<ITransformationModule>();
            foreach (var edge in transformationModule.Graph.Edges)
            {
                if (nodeIds.Contains(edge.Source.Id) || nodeIds.Contains(edge.Target.Id))
                {
                    yield return edge.Source;
                    yield return edge.Target;
                }
            }
        }
    }
}
