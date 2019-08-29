using System.ComponentModel.Composition;
using System.Windows.Controls;
using Plainion.GraphViz.Infrastructure.ViewModel;

namespace Plainion.GraphViz.Modules.Analysis
{
    [Export(typeof(NodeMasksView))]
    partial class NodeMasksView : UserControl
    {
        // designer only
        public NodeMasksView()
            : this(new NodeMasksViewModel(new DefaultDomainModel()))
        {
        }

        [ImportingConstructor]
        public NodeMasksView(NodeMasksViewModel model)
        {
            InitializeComponent();

            DataContext = model;
        }
    }
}
