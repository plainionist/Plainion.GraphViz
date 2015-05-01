using System.Windows;
using Plainion.GraphViz.Model;

namespace Plainion.GraphViz.Visuals
{
    public interface IVisualPicking
    {
        IGraphItem PickMousePosition();
        IGraphItem Pick( Point position );
    }
}
