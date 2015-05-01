using Plainion.GraphViz.Model;

namespace Plainion.GraphViz.Presentation
{
    /// <summary>
    /// Returns true if the given element is visible according to the masks.
    /// </summary>
    public interface IGraphPicking
    {
        bool Pick( Node node );
        bool Pick( Edge edge );
    }
}
