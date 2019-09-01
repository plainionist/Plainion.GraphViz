using System.Windows.Controls;

namespace Plainion.GraphViz.Modules.CodeInspection.CallTree
{
    partial class CallTreeMenuItem : MenuItem
    {
        public CallTreeMenuItem(CallTreeMenuItemModel model )
        {
            InitializeComponent();

            DataContext = model;
        }
    }
}
