using System.ComponentModel.Composition;
using System.Windows.Controls;

namespace Plainion.GraphViz.Modules.CodeInspection.PathFinder
{
    [Export( typeof(PathFinderMenuItem) )]
    partial class PathFinderMenuItem : MenuItem
    {
        [ImportingConstructor]
        public PathFinderMenuItem(PathFinderMenuItemModel model )
        {
            InitializeComponent();

            DataContext = model;
        }
    }
}
