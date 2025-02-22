using System.Windows.Controls;

namespace Plainion.GraphViz.Modules.Analysis.Clusters
{
    partial class ClusterEditor : UserControl
    {
        public ClusterEditor( ClusterEditorModel model )
        {
            InitializeComponent();

            DataContext = model;
        }
    }
}
