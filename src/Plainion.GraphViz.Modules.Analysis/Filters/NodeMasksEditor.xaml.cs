using System.Windows.Controls;

namespace Plainion.GraphViz.Modules.Analysis.Filters
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
