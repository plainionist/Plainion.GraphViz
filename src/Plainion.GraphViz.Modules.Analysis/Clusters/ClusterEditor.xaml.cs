using System.Windows.Controls;

namespace Plainion.GraphViz.Modules.Analysis.Clusters
{
    partial class ClusterEditor : UserControl
    {
        public ClusterEditor(ClusterEditorViewModel model)
        {
            InitializeComponent();

            DataContext = model;
        }
    }
}
