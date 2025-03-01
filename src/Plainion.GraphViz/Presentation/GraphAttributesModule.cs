using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace Plainion.GraphViz.Presentation;

class GraphAttributesModule : AbstractModule<GraphAttribute>, IGraphAttributesModule
{
    private readonly ObservableCollection<GraphAttribute> myAttributes;

    public GraphAttributesModule()
    {
        myAttributes = [
            new GraphAttribute( Dot.LayoutAlgorithm.Auto, "RankDir", "BT"),
            new GraphAttribute( Dot.LayoutAlgorithm.Auto, "Ratio", "Compress"),
            new GraphAttribute( Dot.LayoutAlgorithm.Auto, "RankSep", "2.0 equally"),

            new GraphAttribute( Dot.LayoutAlgorithm.Flow, "RankDir", "LR"),
        ];
    }

    public override IEnumerable<GraphAttribute> Items => myAttributes;
}

