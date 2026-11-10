using System.Windows.Controls;

namespace Plainion.GraphViz.Modules.Analysis.NodeSelection;

partial class NodeSelectionEditor : UserControl
{
    public NodeSelectionEditor(NodeSelectionEditorModel model)
    {
        InitializeComponent();

        DataContext = model;
    }
}
