using Plainion.GraphViz.Presentation;

namespace Plainion.GraphViz
{
    public interface ILayoutEngine
    {
        void Relayout( IGraphPresentation presentation );
    }
}
