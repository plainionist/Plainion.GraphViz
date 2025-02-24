using System.Diagnostics;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Plainion.Windows.Interactivity.DragDrop;

namespace Plainion.GraphViz.Modules.Analysis.Clusters;

partial class TreeEditor : UserControl, IDropable
{
    public TreeEditor()
    {
        InitializeComponent();
    }

    public static DependencyProperty NodeForContextMenuProperty = DependencyProperty.Register("NodeForContextMenu", typeof(NodeViewModel), typeof(TreeEditor),
        new FrameworkPropertyMetadata(null));

    public NodeViewModel NodeForContextMenu
    {
        get { return (NodeViewModel)GetValue(NodeForContextMenuProperty); }
        set { SetValue(NodeForContextMenuProperty, value); }
    }

    protected override void OnPreviewMouseRightButtonDown(MouseButtonEventArgs e)
    {
        NodeForContextMenu = null;

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
            NodeForContextMenu = (NodeViewModel)nodeItem.DataContext;

            nodeItem.Focus();
        }
        else
        {
            // if we click directly into the tree control we pick Root
            NodeForContextMenu = Root;
            Debug.WriteLine("NodeForContextMenu: " + Root);
        }

        RefreshContextMenuCommandsCanExecute();

        e.Handled = true;
    }

    private void RefreshContextMenuCommandsCanExecute()
    {
        foreach (var item in myTree.ContextMenu.Items.OfType<MenuItem>())
        {
            if (item.Command == null)
            {
                continue;
            }

            var raiseMethod = item.Command.GetType().GetMethod("RaiseCanExecuteChanged");
            if (raiseMethod != null)
            {
                raiseMethod.Invoke(item.Command, null);
            }
            else
            {
                item.IsEnabled = item.Command.CanExecute(item.CommandParameter);
            }
        }
    }

    public static DependencyProperty RootProperty = DependencyProperty.Register("Root", typeof(NodeViewModel), typeof(TreeEditor), new FrameworkPropertyMetadata(null));

    public NodeViewModel Root
    {
        get { return (NodeViewModel)GetValue(RootProperty); }
        set { SetValue(RootProperty, value); }
    }

    string IDropable.DataFormat => typeof(NodeView).FullName;

    bool IDropable.IsDropAllowed(object data, DropLocation location) => true;

    void IDropable.Drop(object data, DropLocation _)
    {
        if (data is not NodeView droppedElement)
        {
            return;
        }

        var arg = new NodeDropRequest((NodeViewModel)droppedElement.DataContext, Root);

        if (DropCommand != null && DropCommand.CanExecute(arg))
        {
            DropCommand.Execute(arg);
        }
    }

    public static DependencyProperty DropCommandProperty = DependencyProperty.Register("DropCommand", typeof(ICommand), typeof(TreeEditor),
        new FrameworkPropertyMetadata(null));

    public ICommand DropCommand
    {
        get { return (ICommand)GetValue(DropCommandProperty); }
        set { SetValue(DropCommandProperty, value); }
    }
}
