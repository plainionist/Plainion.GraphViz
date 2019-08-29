using System.ComponentModel.Composition;
using System.Windows.Controls;

namespace Plainion.GraphViz.Modules.Analysis
{
    [Export( typeof( ClusterEditor ) )]
    partial class ClusterEditor : UserControl
    {
        [ImportingConstructor]
        public ClusterEditor( ClusterEditorModel model )
        {
            InitializeComponent();

            DataContext = model;
        }
    }
}
