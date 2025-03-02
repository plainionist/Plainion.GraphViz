using System;
using System.Collections.Generic;
using System.Linq;
using Plainion.Graphs;

namespace Plainion.GraphViz.Algorithms
{
    class Traverse
    {
        public static IEnumerable<Edge> BreathFirst(IEnumerable<Node> roots, Func<Node, IEnumerable<Edge>> siblingsSelector)
        {
            var unvisitedNodes = new HashSet<Node>(roots);
            var visitedNodes = new HashSet<Node>();

            while (unvisitedNodes.Count > 0)
            {
                var currentNodes = unvisitedNodes.ToList();
                unvisitedNodes.Clear();

                foreach (var node in currentNodes)
                {
                    foreach (var edge in siblingsSelector(node))
                    {
                        unvisitedNodes.Add(edge.Source);
                        unvisitedNodes.Add(edge.Target);

                        yield return edge;
                    }

                    visitedNodes.Add(node);
                }

                unvisitedNodes.ExceptWith(visitedNodes);
            }
        }
    }
}
