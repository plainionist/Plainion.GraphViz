using System.Windows.Controls;

namespace Plainion.GraphViz.Modules.CodeInspection.Inheritance
{
    partial class InheritanceGraphBuilderView : UserControl
    {
        public InheritanceGraphBuilderView( InheritanceGraphBuilderViewModel model )
        {
            InitializeComponent();

            DataContext = model;
        }
    }
}
