using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace Plainion.GraphViz.Modules.Analysis.Clusters;

/// <summary>
/// Used by the <see cref="TreeEditor"/> to handle certain aspects which are difficult to handle in the Xaml.
/// </summary>
class ExtendedTreeView : TreeView
{
    private NodeViewModel myRoot;

    public NodeViewModel Root
    {
        get { return myRoot; }
        set
        {
            if (myRoot != value)
            {
                myRoot = value;
            }
        }
    }

    protected override DependencyObject GetContainerForItemOverride()
    {
        return new NodeView();
    }

    protected override bool IsItemItsOwnContainerOverride(object item)
    {
        return item is NodeView;
    }

    public static DependencyProperty NodeForContextMenuProperty = DependencyProperty.Register("NodeForContextMenu", typeof(NodeViewModel), typeof(ExtendedTreeView),
        new FrameworkPropertyMetadata(null));

    public NodeViewModel NodeForContextMenu
    {
        get { return (NodeViewModel)GetValue(NodeForContextMenuProperty); }
        set { SetValue(NodeForContextMenuProperty, value); }
    }

    // TODO: into behavior?
    protected override void OnPreviewMouseRightButtonDown(MouseButtonEventArgs e)
    {
        NodeForContextMenu = null;

        NodeView nodeItem = null;

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
            NodeForContextMenu = (NodeViewModel)nodeItem.DataContext;

            nodeItem.Focus();
        }
        else
        {
            // if we click directly into the tree control we pick Root
            NodeForContextMenu = Root;
        }

        RefreshContextMenuCommandsCanExecute();

        e.Handled = true;
    }

    private void RefreshContextMenuCommandsCanExecute()
    {
        foreach (var item in ContextMenu.Items.OfType<MenuItem>())
        {
            var command = item.Command;
            if (command != null)
            {
                var raiseMethod = command.GetType().GetMethod("RaiseCanExecuteChanged");
                if (raiseMethod != null)
                {
                    raiseMethod.Invoke(command, null);
                }
                else
                {
                    item.IsEnabled = command.CanExecute(item.CommandParameter);
                }
            }
        }
    }
}
