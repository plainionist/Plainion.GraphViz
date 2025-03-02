using System.Windows;
using Plainion.Graphs;

namespace Plainion.GraphViz.Visuals
{
    public interface IVisualPicking
    {
        IGraphItem PickMousePosition();
        IGraphItem Pick( Point position );
    }
}
