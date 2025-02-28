using System.Windows.Controls;
using Prism.Navigation.Regions;

namespace Plainion.GraphViz.Modules.CodeInspection.CallTree
{
    [ViewSortHint("1tool-0010")]
    partial class CallTreeMenuItem : MenuItem
    {
        public CallTreeMenuItem(CallTreeMenuItemModel model)
        {
            InitializeComponent();

            DataContext = model;
        }
    }
}
