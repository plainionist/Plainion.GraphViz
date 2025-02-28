using System.Windows.Controls;
using Prism.Navigation.Regions;

namespace Plainion.GraphViz.Modules.CodeInspection.PathFinder
{
    [ViewSortHint("tool-0040")]
    partial class PathFinderMenuItem : MenuItem
    {
        public PathFinderMenuItem(PathFinderMenuItemModel model )
        {
            InitializeComponent();

            DataContext = model;
        }
    }
}
