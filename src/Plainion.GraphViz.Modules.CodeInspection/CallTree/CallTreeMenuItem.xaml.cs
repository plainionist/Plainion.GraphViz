using System.ComponentModel.Composition;
using System.Windows.Controls;

namespace Plainion.GraphViz.Modules.CodeInspection.CallTree
{
    [Export( typeof(CallTreeMenuItem) )]
    public partial class CallTreeMenuItem : MenuItem
    {
        [ImportingConstructor]
        public CallTreeMenuItem(CallTreeMenuItemModel model )
        {
            InitializeComponent();

            DataContext = model;
        }
    }
}
