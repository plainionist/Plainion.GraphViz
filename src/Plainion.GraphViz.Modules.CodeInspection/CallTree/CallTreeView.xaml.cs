using System.ComponentModel;
using System.ComponentModel.Composition;
using System.Windows;
using System.Windows.Controls;

namespace Plainion.GraphViz.Modules.CodeInspection.CallTree
{
    [Export]
    public partial class CallTreeView : UserControl
    {
        [ImportingConstructor]
        internal CallTreeView( CallTreeViewModel model )
        {
            InitializeComponent();

            DataContext = model;
        }
    }
}
