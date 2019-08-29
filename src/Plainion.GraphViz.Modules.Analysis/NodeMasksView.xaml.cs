using System.ComponentModel.Composition;
using System.Windows.Controls;

namespace Plainion.GraphViz.Modules.Analysis
{
    [Export( typeof( NodeMasksView ) )]
    partial class NodeMasksView : UserControl
    {
        public NodeMasksView()
            : this( new NodeMasksViewModel() )
        {
        }

        [ImportingConstructor]
        public NodeMasksView( NodeMasksViewModel model )
        {
            InitializeComponent();

            DataContext = model;
        }
    }
}
