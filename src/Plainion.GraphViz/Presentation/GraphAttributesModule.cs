using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Plainion.GraphViz.Dot;

namespace Plainion.GraphViz.Presentation;

class GraphAttributesModule : AbstractModule<GraphAttribute>, IGraphAttributesModule
{
    private readonly List<GraphAttribute> myAttributes;

    public GraphAttributesModule()
    {
        myAttributes = [
            new GraphAttribute(LayoutAlgorithm.Hierarchy, "RankDir", "BT"),
            new GraphAttribute(LayoutAlgorithm.Hierarchy, "Ratio", "Compress"),
            new GraphAttribute(LayoutAlgorithm.Hierarchy, "RankSep", "2.0 equally"),

            new GraphAttribute(LayoutAlgorithm.Flow, "RankDir", "LR"),

            new GraphAttribute(LayoutAlgorithm.ScalableForcceDirectedPlancement, "overlap", "prism"),
            new GraphAttribute(LayoutAlgorithm.ScalableForcceDirectedPlancement, "start", "rand"),
            new GraphAttribute(LayoutAlgorithm.ScalableForcceDirectedPlancement, "splines", "true"),
            new GraphAttribute(LayoutAlgorithm.ScalableForcceDirectedPlancement, "edgeweight", "2"),
            new GraphAttribute(LayoutAlgorithm.ScalableForcceDirectedPlancement, "K", "0.5"),
            new GraphAttribute(LayoutAlgorithm.ScalableForcceDirectedPlancement, "compound", "true"),
            new GraphAttribute(LayoutAlgorithm.ScalableForcceDirectedPlancement, "sep", "0.1"),
            new GraphAttribute(LayoutAlgorithm.ScalableForcceDirectedPlancement, "nodesep", "0.2"),
            new GraphAttribute(LayoutAlgorithm.ScalableForcceDirectedPlancement, "normalize", "true"),

            new GraphAttribute(LayoutAlgorithm.ForceDirectedPlacement, "overlap", "prism"),
            new GraphAttribute(LayoutAlgorithm.ForceDirectedPlacement, "start", "rand"),
            new GraphAttribute(LayoutAlgorithm.ForceDirectedPlacement, "splines", "true"),
            new GraphAttribute(LayoutAlgorithm.ForceDirectedPlacement, "packmode", "graph"),
            new GraphAttribute(LayoutAlgorithm.ForceDirectedPlacement, "nodesep", "0.3"),
            new GraphAttribute(LayoutAlgorithm.ForceDirectedPlacement, "sep", "0.2"),
            new GraphAttribute(LayoutAlgorithm.ForceDirectedPlacement, "K", "0.7"),
            new GraphAttribute(LayoutAlgorithm.ForceDirectedPlacement, "normalize", "true"),
            new GraphAttribute(LayoutAlgorithm.ForceDirectedPlacement, "edgeweight", "2"),

            new GraphAttribute(LayoutAlgorithm.NeatSpring, "mode", "major"),
            new GraphAttribute(LayoutAlgorithm.NeatSpring, "compound", "true"),
            new GraphAttribute(LayoutAlgorithm.NeatSpring, "overlap", "prism"),
            new GraphAttribute(LayoutAlgorithm.NeatSpring, "nodesep", "0.5"),
            new GraphAttribute(LayoutAlgorithm.NeatSpring, "sep", "0.5"),
            new GraphAttribute(LayoutAlgorithm.NeatSpring, "start", "rand"),
            new GraphAttribute(LayoutAlgorithm.NeatSpring, "normalize", "true"),
            new GraphAttribute(LayoutAlgorithm.NeatSpring, "edgeweight", "2"),
        ];
    }

    public override IEnumerable<GraphAttribute> Items => myAttributes;

    public IEnumerable<GraphAttribute> ItemsFor(LayoutAlgorithm algo)
    {
        if (algo == LayoutAlgorithm.Auto)
        {
            algo = LayoutAlgorithm.Hierarchy;
        }

        return myAttributes
            .Where(x => x.Algorithm == algo)
            .Where(x => x.Value != null);
    }
}
