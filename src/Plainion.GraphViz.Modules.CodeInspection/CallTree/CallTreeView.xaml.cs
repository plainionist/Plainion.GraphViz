using System.Windows.Controls;

namespace Plainion.GraphViz.Modules.CodeInspection.CallTree
{
    partial class CallTreeView : UserControl
    {
        public CallTreeView( CallTreeViewModel model )
        {
            InitializeComponent();

            DataContext = model;
        }
    }
}
