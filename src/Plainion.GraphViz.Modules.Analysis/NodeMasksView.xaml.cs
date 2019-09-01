using System.Windows.Controls;
using Plainion.GraphViz.Infrastructure.ViewModel;

namespace Plainion.GraphViz.Modules.Analysis
{
    partial class NodeMasksView : UserControl
    {
        // designer only
        public NodeMasksView()
            : this(new NodeMasksViewModel(new DefaultDomainModel()))
        {
        }

        public NodeMasksView(NodeMasksViewModel model)
        {
            InitializeComponent();

            DataContext = model;
        }
    }
}
