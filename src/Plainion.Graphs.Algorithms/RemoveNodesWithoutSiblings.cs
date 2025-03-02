using System;
using System.Linq;
using Plainion.GraphViz.Presentation;

namespace Plainion.Graphs.Algorithms;

/// <summary>
/// Generates a "hide mask" removing all visible nodes not having siblings of the given type.
/// </summary>
public class RemoveNodesWithoutSiblings : AbstractAlgorithm
{
    public RemoveNodesWithoutSiblings(IGraphPresentation presentation)
        : base(presentation)
    {
        SiblingsType = SiblingsType.Any;
    }

    public SiblingsType SiblingsType { get; set; }

    public INodeMask Compute()
    {
        var transformationModule = Presentation.GetModule<ITransformationModule>();
        var nodesToHide = transformationModule.Graph.Nodes
            // do not process hidden nodes
            .Where(Presentation.Picking.Pick)
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
            return node.In.All(e => !Presentation.Picking.Pick(e.Source)) && node.Out.All(e => !Presentation.Picking.Pick(e.Target));
        }
        else if (SiblingsType == SiblingsType.Sources)
        {
            return node.In.All(e => !Presentation.Picking.Pick(e.Source));
        }
        else if (SiblingsType == SiblingsType.Targets)
        {
            return node.Out.All(e => !Presentation.Picking.Pick(e.Target));
        }
        else
        {
            throw new NotSupportedException(SiblingsType.ToString());
        }
    }
}
