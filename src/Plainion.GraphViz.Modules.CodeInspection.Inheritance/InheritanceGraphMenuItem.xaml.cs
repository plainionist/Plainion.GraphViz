using System.Windows.Controls;
using Prism.Navigation.Regions;

namespace Plainion.GraphViz.Modules.CodeInspection.Inheritance
{
    [ViewSortHint("tool-0020")]
    partial class InheritanceGraphMenuItem : MenuItem
    {
        public InheritanceGraphMenuItem( InheritanceGraphMenuItemModel model )
        {
            InitializeComponent();

            DataContext = model;
        }
    }
}
