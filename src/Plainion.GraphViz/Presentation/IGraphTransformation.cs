using Plainion.Graphs;

namespace Plainion.GraphViz.Presentation
{
    public interface IGraphTransformation
    {
        IGraph Transform( IGraph graph );
    }
}
