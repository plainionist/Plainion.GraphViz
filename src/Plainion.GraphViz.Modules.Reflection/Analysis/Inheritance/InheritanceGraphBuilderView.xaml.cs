using System.ComponentModel;
using System.ComponentModel.Composition;
using System.Windows;
using System.Windows.Controls;

namespace Plainion.GraphViz.Modules.Reflection.Analysis.Inheritance
{
    [Export( typeof( InheritanceGraphBuilderView ) )]
    public partial class InheritanceGraphBuilderView : UserControl
    {
        [ImportingConstructor]
        internal InheritanceGraphBuilderView( InheritanceGraphBuilderViewModel model )
        {
            InitializeComponent();

            DataContext = model;
        }
    }
}
