using System.ComponentModel.Composition;
using System.Windows.Controls;

namespace Plainion.GraphViz.Modules.CodeInspection.Inheritance
{
    [Export( typeof( InheritanceGraphBuilderView ) )]
    partial class InheritanceGraphBuilderView : UserControl
    {
        [ImportingConstructor]
        public InheritanceGraphBuilderView( InheritanceGraphBuilderViewModel model )
        {
            InitializeComponent();

            DataContext = model;
        }
    }
}
