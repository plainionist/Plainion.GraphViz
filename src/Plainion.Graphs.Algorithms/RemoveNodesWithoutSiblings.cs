using System;
using System.Linq;
using Plainion.Graphs.Projections;

namespace Plainion.Graphs.Algorithms;

/// <summary>
/// Generates a "hide mask" removing all visible nodes not having siblings of the given type.
/// </summary>
public class RemoveNodesWithoutSiblings : AbstractAlgorithm
{
    public RemoveNodesWithoutSiblings(IGraphProjections projections)
        : base(projections)
    {
        SiblingsType = SiblingsType.Any;
    }

    public SiblingsType SiblingsType { get; set; }

    public INodeMask Compute()
    {
        var nodesToHide = Projections.TransformedGraph.Nodes
            // do not process hidden nodes
            .Where(Projections.Picking.Pick)
            .Where(n => HideNode(n));

        var mask = new NodeMask();
        mask.IsShowMask = false;
        mask.Set(nodesToHide);

        if (SiblingsType == SiblingsType.Any)
        {
            mask.Label = "Nodes without siblings";
        }
        else if (SiblingsType == SiblingsType.Sources)
        {
            mask.Label = "Nodes without sources";
        }
        else if (SiblingsType == SiblingsType.Targets)
        {
            mask.Label = "Nodes without targets";
        }

        return mask;
    }

    private bool HideNode(Node node)
    {
        if (SiblingsType == SiblingsType.Any)
        {
            return node.In.All(e => !Projections.Picking.Pick(e.Source)) && node.Out.All(e => !Projections.Picking.Pick(e.Target));
        }
        else if (SiblingsType == SiblingsType.Sources)
        {
            return node.In.All(e => !Projections.Picking.Pick(e.Source));
        }
        else if (SiblingsType == SiblingsType.Targets)
        {
            return node.Out.All(e => !Projections.Picking.Pick(e.Target));
        }
        else
        {
            throw new NotSupportedException(SiblingsType.ToString());
        }
    }
}
