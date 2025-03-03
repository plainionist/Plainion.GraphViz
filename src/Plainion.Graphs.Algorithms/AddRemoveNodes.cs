using System;
using System.Collections.Generic;
using System.Linq;
using Plainion.Graphs.Projections;

namespace Plainion.Graphs.Algorithms;

/// <summary>
/// Generates hide mask of all nodes given.
/// </summary>
public class AddRemoveNodes : AbstractAlgorithm
{
    public AddRemoveNodes(IGraphProjections presentation)
        : base(presentation)
    {
        SiblingsType = SiblingsType.None;
    }

    public bool Add { get; set; }

    public SiblingsType SiblingsType { get; set; }

    public INodeMask Compute(params Node[] nodes)
    {
        return Compute((IEnumerable<Node>)nodes);
    }

    public INodeMask Compute(IEnumerable<Node> nodes)
    {
        var mask = new NodeMask();
        mask.IsShowMask = Add;

        mask.Set(nodes.SelectMany(SelectNodes));

        if (SiblingsType == SiblingsType.Sources)
        {
            mask.Label = "Sources of ";
        }
        else if (SiblingsType == SiblingsType.Targets)
        {
            mask.Label = "Targets of ";
        }
        else if (SiblingsType == SiblingsType.Any)
        {
            mask.Label = "Siblings of ";
        }

        mask.Label += Projections.GetCaption(nodes.First().Id);

        if (nodes.Count() > 1)
        {
            mask.Label += " ...";
        }

        return mask;
    }

    private IEnumerable<Node> SelectNodes(Node node)
    {
        if (SiblingsType == SiblingsType.None)
        {
            return new[] { node };
        }
        else if (SiblingsType == SiblingsType.Sources)
        {
            return node.In
                .Select(e => e.Source)
                .Where(n => Projections.Picking.Pick(n) != Add);
        }
        else if (SiblingsType == SiblingsType.Targets)
        {
            return node.Out
                .Select(e => e.Target)
                .Where(n => Projections.Picking.Pick(n) != Add);
        }
        else if (SiblingsType == SiblingsType.Any)
        {
            return node.In.Select(e => e.Source)
                .Concat(node.Out.Select(e => e.Target))
                .Where(n => Projections.Picking.Pick(n) != Add);
        }
        else
        {
            throw new NotSupportedException(SiblingsType.ToString());
        }
    }
}
