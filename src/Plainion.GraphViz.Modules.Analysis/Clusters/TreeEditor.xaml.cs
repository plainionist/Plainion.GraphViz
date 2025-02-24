using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Plainion.Windows.Interactivity.DragDrop;

namespace Plainion.GraphViz.Modules.Analysis.Clusters;

partial class TreeEditor : UserControl
{
    public TreeEditor()
    {
        InitializeComponent();
    }

    private TreeEditorViewModel ViewModel => (TreeEditorViewModel)DataContext;

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

        if (nodeItem != null)
        {
            ViewModel.NodeForContextMenu = (NodeViewModel)nodeItem.DataContext;

            nodeItem.Focus();
        }
        else
        {
            // if we click directly into the tree control we pick Root
            ViewModel.NodeForContextMenu = Root;
        }

        e.Handled = true;
    }

    public static DependencyProperty RootProperty = DependencyProperty.Register("Root", typeof(NodeViewModel), typeof(TreeEditor), new FrameworkPropertyMetadata(null));

    public NodeViewModel Root
    {
        get { return (NodeViewModel)GetValue(RootProperty); }
        set { SetValue(RootProperty, value); }
    }
}
