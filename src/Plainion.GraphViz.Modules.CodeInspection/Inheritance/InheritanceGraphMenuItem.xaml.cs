using System.Windows.Controls;

namespace Plainion.GraphViz.Modules.CodeInspection.Inheritance
{
    partial class InheritanceGraphMenuItem : MenuItem
    {
        public InheritanceGraphMenuItem( InheritanceGraphMenuItemModel model )
        {
            InitializeComponent();

            DataContext = model;
        }
    }
}
