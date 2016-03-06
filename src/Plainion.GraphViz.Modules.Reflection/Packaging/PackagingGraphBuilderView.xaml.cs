using System.ComponentModel.Composition;
using System.Windows.Controls;

namespace Plainion.GraphViz.Modules.Reflection.Packaging
{
    [Export(typeof(PackagingGraphBuilderView))]
    public partial class PackagingGraphBuilderView : UserControl
    {
        [ImportingConstructor]
        internal PackagingGraphBuilderView(PackagingGraphBuilderViewModel model)
        {
            InitializeComponent();

            DataContext = model;
        }
    }
}
