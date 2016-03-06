using System.ComponentModel.Composition;
using System.Windows.Controls;

namespace Plainion.GraphViz.Modules.CodeInspection.Packaging
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
