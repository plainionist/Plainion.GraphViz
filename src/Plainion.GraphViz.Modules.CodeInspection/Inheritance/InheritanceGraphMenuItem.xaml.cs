using System.ComponentModel.Composition;
using System.Windows.Controls;

namespace Plainion.GraphViz.Modules.CodeInspection.Inheritance
{
    [Export( typeof( InheritanceGraphMenuItem ) )]
    partial class InheritanceGraphMenuItem : MenuItem
    {
        [ImportingConstructor]
        public InheritanceGraphMenuItem( InheritanceGraphMenuItemModel model )
        {
            InitializeComponent();

            DataContext = model;
        }
    }
}
