using System.Windows.Controls;

namespace Plainion.GraphViz.Modules.Analysis
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
