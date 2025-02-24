using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace Plainion.GraphViz.Modules.Analysis.Clusters;

partial class TreeEditor : UserControl
{
    public TreeEditor()
    {
        InitializeComponent();
    }

    protected override void OnPreviewMouseRightButtonDown(MouseButtonEventArgs e)
    {
        NodeView nodeItem;

        if (Keyboard.Modifiers == ModifierKeys.Control)
        {
            // in case the full treeview is filled with nodes there is no option to 
            // open context menu without selecting any node (because nodes stretch horizontally)
            // -> use a modifier to signal that context menu should be opened with the full tree as context
            nodeItem = null;
        }
        else
        {
            nodeItem = ((DependencyObject)e.OriginalSource).FindParentOfType<NodeView>();
        }

        var viewModel = (TreeEditorViewModel)DataContext;

        if (nodeItem != null)
        {
            viewModel.NodeForContextMenu = (NodeViewModel)nodeItem.DataContext;

            nodeItem.Focus();
        }
        else
        {
            // if we click directly into the tree control we pick Root
            viewModel.NodeForContextMenu = null;
        }

        e.Handled = true;
    }
}
