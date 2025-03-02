using System.Collections.Generic;
using Plainion.Graphs;
using Plainion.GraphViz.Dot;

namespace Plainion.GraphViz.Presentation
{
    public interface IGraphLayoutModule : IModule<AbstractPropertySet>
    {
        LayoutAlgorithm Algorithm { get; set; }

        void Add(NodeLayout layout);
        void Add(EdgeLayout layout);
        void Clear();

        NodeLayout GetLayout(Node node);
        EdgeLayout GetLayout(Edge edge);

        void Set(IEnumerable<NodeLayout> nodeLayouts, IEnumerable<EdgeLayout> edgeLayouts);
    }
}
