using System.Windows.Controls;

namespace Plainion.GraphViz.Modules.Analysis
{
    partial class NodeMasksEditor : UserControl
    {
        public NodeMasksEditor( NodeMasksEditorModel model )
        {
            InitializeComponent();

            DataContext = model;
        }
    }
}
