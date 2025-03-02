using System;
using System.Collections.Generic;
using System.Linq;
using Plainion.Graphs;
using Plainion.Graphs.Algorithms;
using Plainion.GraphViz.Presentation;

namespace Plainion.Graphs.Algorithms;

/// <summary>
/// Generates "hide mask" containing all visible nodes NOT on the path between the two given nodes.
/// </summary>
public class ShowPath : AbstractAlgorithm
{
    public ShowPath(IGraphPresentation presentation)
        : base(presentation)
    {
    }

    public INodeMask Compute(Node from, Node to)
    {
        var captions = Presentation.GetModule<ICaptionModule>();

        var mask = new NodeMask();
        mask.IsShowMask = false;
        mask.Label = $"Path from {captions.Get(from.Id).DisplayText} to {captions.Get(to.Id).DisplayText}";

        mask.Set(GetPaths(from, to));
        mask.Invert(Presentation);

        return mask;
    }

    private IEnumerable<Node> GetPaths(Node source, Node target)
    {
        var reachableFromSource = GetReachableNodes(source, n => n.Out.Where(e => Presentation.Picking.Pick(e.Target)));
        var reachingTarget = GetReachableNodes(target, n => n.In.Where(e => Presentation.Picking.Pick(e.Source)));

        return reachableFromSource
            .Intersect(reachingTarget)
            .ToList();
    }

    private IEnumerable<Node> GetReachableNodes(Node node, Func<Node, IEnumerable<Edge>> selector)
    {
        var connectedNodes = new HashSet<Node>();
        connectedNodes.Add(node);

        var recursiveSiblings = Traverse.BreathFirst(new[] { node }, selector)
            .SelectMany(e => new[] { e.Source, e.Target });

        foreach (var n in recursiveSiblings)
        {
            connectedNodes.Add(n);
        }

        return connectedNodes;
    }
}
