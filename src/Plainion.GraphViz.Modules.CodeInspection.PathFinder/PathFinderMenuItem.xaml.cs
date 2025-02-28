using System.Windows.Controls;
using Prism.Navigation.Regions;

namespace Plainion.GraphViz.Modules.CodeInspection.PathFinder
{
    [ViewSortHint(Viewer.Abstractions.RegionNames.AddIns + ".0040")]
    partial class PathFinderMenuItem : MenuItem
    {
        public PathFinderMenuItem(PathFinderMenuItemModel model )
        {
            InitializeComponent();

            DataContext = model;
        }
    }
}
