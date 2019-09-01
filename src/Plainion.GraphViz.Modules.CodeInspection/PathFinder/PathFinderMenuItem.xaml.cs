using System.Windows.Controls;

namespace Plainion.GraphViz.Modules.CodeInspection.PathFinder
{
    partial class PathFinderMenuItem : MenuItem
    {
        public PathFinderMenuItem(PathFinderMenuItemModel model )
        {
            InitializeComponent();

            DataContext = model;
        }
    }
}
