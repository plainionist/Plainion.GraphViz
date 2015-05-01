using System.ComponentModel.Composition;
using System.Windows.Controls;

namespace Plainion.GraphViz.Modules.Analysis
{
    [Export( typeof( NodeMasksView ) )]
    public partial class NodeMasksView : UserControl
    {
        public NodeMasksView()
            : this( new NodeMasksViewModel() )
        {
        }

        [ImportingConstructor]
        internal NodeMasksView( NodeMasksViewModel model )
        {
            InitializeComponent();

            DataContext = model;
        }
    }
}
