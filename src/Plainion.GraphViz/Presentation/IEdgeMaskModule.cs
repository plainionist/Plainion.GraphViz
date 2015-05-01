using Plainion.GraphViz.Model;

namespace Plainion.GraphViz.Presentation
{
    public interface IEdgeMaskModule : IModule<Edge>
    {
        void Add( Edge edge );
        void Remove( Edge edge );
    }
}
