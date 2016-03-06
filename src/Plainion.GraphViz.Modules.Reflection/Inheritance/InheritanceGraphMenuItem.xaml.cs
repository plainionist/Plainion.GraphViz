using System.ComponentModel.Composition;
using System.Windows.Controls;

namespace Plainion.GraphViz.Modules.Reflection.Inheritance
{
    [Export( typeof( InheritanceGraphMenuItem ) )]
    public partial class InheritanceGraphMenuItem : MenuItem
    {
        [ImportingConstructor]
        public InheritanceGraphMenuItem( InheritanceGraphMenuItemModel model )
        {
            InitializeComponent();

            DataContext = model;
        }
    }
}
