using System.Windows.Controls;

namespace Plainion.GraphViz.Modules.CodeInspection.PathFinder
{
    partial class PathFinderView : UserControl
    {
        public PathFinderView( PathFinderViewModel model )
        {
            InitializeComponent();

            DataContext = model;
        }
    }
}
