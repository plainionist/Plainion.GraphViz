using System.Collections.Generic;
using System.Linq;
using Plainion.GraphViz.Model;
using Plainion.GraphViz.Presentation;

namespace Plainion.GraphViz.Algorithms
{
    /// <summary>
    /// Shows all nodes which have only a single incoming edge.
    /// </summary>
    public class ShowNodesWithSingleIncoming
    {
        private readonly IGraphPresentation myPresentation;

        public ShowNodesWithSingleIncoming(IGraphPresentation presentation)
        {
            Contract.RequiresNotNull(presentation, "presentation");

            myPresentation = presentation;
        }

        public void Execute()
        {
            var mask = new NodeMask();
            mask.Label = "Nodes with singe incomings";
            mask.IsShowMask = true;

            var transformationModule = myPresentation.GetModule<ITransformationModule>();

            var relevantNodes = transformationModule.Graph.Nodes
                .Where(n => n.In.Count == 1);

            foreach (var node in relevantNodes)
            {
                mask.Set(node);
                mask.Set(node.In.Single().Source);
            }

            var module = myPresentation.GetModule<INodeMaskModule>();
            module.Push(mask);
        }
    }
}
