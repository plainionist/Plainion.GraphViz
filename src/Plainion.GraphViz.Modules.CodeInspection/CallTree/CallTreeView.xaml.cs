using System.ComponentModel.Composition;
using System.Windows.Controls;

namespace Plainion.GraphViz.Modules.CodeInspection.CallTree
{
    [Export]
    partial class CallTreeView : UserControl
    {
        [ImportingConstructor]
        public CallTreeView( CallTreeViewModel model )
        {
            InitializeComponent();

            DataContext = model;
        }
    }
}
