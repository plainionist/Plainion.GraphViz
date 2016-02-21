using System.ComponentModel.Composition;
using System.Windows.Controls;

namespace Plainion.GraphViz.Modules.Reflection.Analysis
{
    [Export( typeof( TypeDependencyGraphMenuItem ) )]
    public partial class TypeDependencyGraphMenuItem : MenuItem
    {
        [ImportingConstructor]
        public TypeDependencyGraphMenuItem( TypeDependencyGraphMenuItemModel model )
        {
            InitializeComponent();

            DataContext = model;
        }
    }
}
