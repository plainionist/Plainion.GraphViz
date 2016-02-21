using System.ComponentModel;
using System.ComponentModel.Composition;
using System.Windows;
using System.Windows.Controls;

namespace Plainion.GraphViz.Modules.Reflection.Analysis
{
    [Export( typeof( TypeDependencyGraphBuilderView ) )]
    public partial class TypeDependencyGraphBuilderView : UserControl
    {
        [ImportingConstructor]
        internal TypeDependencyGraphBuilderView( TypeDependencyGraphBuilderViewModel model )
        {
            InitializeComponent();

            DataContext = model;
        }
    }
}
