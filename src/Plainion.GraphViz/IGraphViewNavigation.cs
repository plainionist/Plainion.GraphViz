using Plainion.GraphViz.Model;

namespace Plainion.GraphViz
{
    public interface IGraphViewNavigation
    {
        void NavigateTo(IGraphItem item);
        void HomeZoomPan();
    }
}
