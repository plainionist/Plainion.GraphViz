using Plainion.GraphViz.Model;

namespace Plainion.GraphViz.Presentation
{
    public interface IGraphTransformation
    {
        IGraph Transform( IGraph myGraph );
    }
}
