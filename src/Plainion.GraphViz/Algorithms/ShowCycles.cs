using System.Linq;
using Plainion.GraphViz.Model;
using Plainion.GraphViz.Presentation;
using Plainion;
using System.Collections.Generic;


namespace Plainion.GraphViz.Algorithms
{
    public class ShowCycles
    {
        private IGraphPresentation myPresentation;

        public ShowCycles(IGraphPresentation presentation)
        {
            Contract.RequiresNotNull(presentation, "presentation");

            myPresentation = presentation;
        }

        public void Execute()
        {
            var mask = new NodeMask();
            mask.Label = "Cycles";
            mask.IsShowMask = true;

            try
            {
                if (!myPresentation.Graph.Nodes.Any())
                {
                    return;
                }

                var unvisited = new HashSet<Node>(myPresentation.Graph.Nodes);
                unvisited.RemoveWhere(n => n.In.Count == 0 || n.Out.Count == 0);

                while (unvisited.Count > 0)
                {
                    var current = unvisited.First();
                    unvisited.Remove(current);

                    foreach (var node in FindCycles(unvisited, current, new HashSet<Node> { current }))
                    {
                        mask.Set(node);
                    }
                }
            }
            finally
            {
                var module = myPresentation.GetModule<INodeMaskModule>();
                module.Push(mask);
            }
        }

        private static IEnumerable<IList<Node>> FindCycles(HashSet<Node> unvisited, Node current, HashSet<Node> visited)
        {
            foreach (var inNode in current.In.Select(edge => edge.Source))
            {
                if (visited.Contains(inNode))
                {
                    yield return new List<Node> { inNode, current };
                }
                else
                {
                    var visitedCopy = new HashSet<Node>(visited);
                    visitedCopy.Add(inNode);
                    unvisited.Remove(inNode);

                    foreach (var cycle in FindCycles(unvisited, inNode, visitedCopy))
                    {
                        if (cycle.First() != cycle.Last())
                        {
                            cycle.Add(current);
                        }

                        yield return cycle;
                    }
                }
            }
        }
    }
}
