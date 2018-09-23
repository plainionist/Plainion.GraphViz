using System.ComponentModel;
using System.ComponentModel.Composition;
using System.Windows;
using System.Windows.Controls;

namespace Plainion.GraphViz.Modules.CodeInspection.PathFinder
{
    [Export]
    public partial class PathFinderView : UserControl
    {
        [ImportingConstructor]
        internal PathFinderView( PathFinderViewModel model )
        {
            InitializeComponent();

            DataContext = model;
        }
    }
}
