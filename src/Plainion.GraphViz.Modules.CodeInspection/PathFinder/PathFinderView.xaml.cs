using System.ComponentModel;
using System.ComponentModel.Composition;
using System.Windows;
using System.Windows.Controls;

namespace Plainion.GraphViz.Modules.CodeInspection.PathFinder
{
    [Export]
    partial class PathFinderView : UserControl
    {
        [ImportingConstructor]
        public PathFinderView( PathFinderViewModel model )
        {
            InitializeComponent();

            DataContext = model;
        }
    }
}
