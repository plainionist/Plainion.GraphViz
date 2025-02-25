using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Shapes;

namespace Plainion.GraphViz.Modules.Analysis.Clusters;

// MultiSelect support based on: https://stackoverflow.com/questions/459375/customizing-the-treeview-to-allow-multi-select
class ExtendedTreeView : TreeView
{
    private NodeView myLastItemSelected;

    protected override DependencyObject GetContainerForItemOverride() => new NodeView();

    protected override bool IsItemItsOwnContainerOverride(object item) => item is NodeView;

    public static readonly DependencyProperty IsItemSelectedProperty = DependencyProperty.RegisterAttached("IsItemSelected", typeof(bool), typeof(ExtendedTreeView));
    public static void SetIsItemSelected(UIElement element, bool value) => element.SetValue(IsItemSelectedProperty, value);
    public static bool GetIsItemSelected(UIElement element) => (bool)element.GetValue(IsItemSelectedProperty);

    protected override void OnPreviewMouseRightButtonDown(MouseButtonEventArgs e)
    {
        NodeView selectedItem;

        if (Keyboard.Modifiers == ModifierKeys.Control)
        {
            // in case the full treeview is filled with nodes there is no option to 
            // open context menu without selecting any node (because nodes stretch horizontally)
            // -> use a modifier to signal that context menu should be opened with the full tree as context
            selectedItem = null;
        }
        else
        {
            selectedItem = ((DependencyObject)e.OriginalSource).FindParentOfType<NodeView>();
        }

        var viewModel = (TreeEditorViewModel)DataContext;

        if (selectedItem != null)
        {
            viewModel.NodeForContextMenu = (NodeViewModel)selectedItem.DataContext;

            SetIsItemSelected(selectedItem, true);
            selectedItem.Focus();
        }
        else
        {
            // if we click directly into the tree control we pick Root
            viewModel.NodeForContextMenu = null;
        }

        e.Handled = true;
    }

    protected override void OnPreviewMouseLeftButtonDown(MouseButtonEventArgs e)
    {
        base.OnPreviewMouseDown(e);

        // If clicking on a tree branch expander...
        if (e.OriginalSource is Shape || e.OriginalSource is Grid || e.OriginalSource is Border)
        {
            return;
        }

        var item = ((FrameworkElement)e.OriginalSource).FindParentOfType<NodeView>();
        if (item != null)
        {
            SelectedItemChangedInternal(item);
        }
    }

    private void SelectedItemChangedInternal(NodeView selectedItem)
    {
        // Clear all previous selected item states if ctrl is NOT being held down
        if (!(Keyboard.IsKeyDown(Key.LeftCtrl) || Keyboard.IsKeyDown(Key.RightCtrl)))
        {
            foreach (var item in GetTreeViewItems(this, true))
            {
                SetIsItemSelected(item, false);
            }
        }

        // Is this an item range selection?
        if ((Keyboard.IsKeyDown(Key.LeftShift) || Keyboard.IsKeyDown(Key.RightShift)) && myLastItemSelected != null)
        {
            var items = GetTreeViewItemRange(myLastItemSelected, selectedItem);
            if (items.Count > 0)
            {
                foreach (var item in items)
                {
                    SetIsItemSelected(item, true);
                }

                myLastItemSelected = items.Last();
            }
        }
        // Otherwise, individual selection
        else
        {
            SetIsItemSelected(selectedItem, !GetIsItemSelected(selectedItem));
            myLastItemSelected = selectedItem;
        }
    }

    private static List<NodeView> GetTreeViewItems(ItemsControl parentItem, bool includeCollapsedItems, List<NodeView> itemList = null)
    {
        itemList ??= [];

        for (var index = 0; index < parentItem.Items.Count; index++)
        {
            var tvItem = parentItem.ItemContainerGenerator.ContainerFromIndex(index) as NodeView;
            if (tvItem == null) continue;

            itemList.Add(tvItem);
            if (includeCollapsedItems || tvItem.IsExpanded)
            {
                GetTreeViewItems(tvItem, includeCollapsedItems, itemList);
            }
        }

        return itemList;
    }

    private List<NodeView> GetTreeViewItemRange(NodeView start, NodeView end)
    {
        var items = GetTreeViewItems(this, false);

        var startIndex = items.IndexOf(start);
        var endIndex = items.IndexOf(end);
        var rangeStart = startIndex > endIndex || startIndex == -1 ? endIndex : startIndex;
        var rangeCount = startIndex > endIndex ? startIndex - endIndex + 1 : endIndex - startIndex + 1;

        if (startIndex == -1 && endIndex == -1)
        {
            rangeCount = 0;
        }
        else if (startIndex == -1 || endIndex == -1)
        {
            rangeCount = 1;
        }

        return rangeCount > 0 ? items.GetRange(rangeStart, rangeCount) : [];
    }
}
