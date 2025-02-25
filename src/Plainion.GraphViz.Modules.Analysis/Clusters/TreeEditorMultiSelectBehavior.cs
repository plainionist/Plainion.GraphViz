using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Microsoft.Xaml.Behaviors;

namespace Plainion.GraphViz.Modules.Analysis.Clusters;

class TreeEditorMultiSelectBehavior : Behavior<TreeView>
{
    private NodeView myLastSelectedItem;

    public static readonly DependencyProperty SelectedItemsProperty =
        DependencyProperty.Register(nameof(SelectedItems), typeof(IList<NodeViewModel>), typeof(TreeEditorMultiSelectBehavior),
            new PropertyMetadata(null));

    public IList<NodeViewModel> SelectedItems
    {
        get => (IList<NodeViewModel>)GetValue(SelectedItemsProperty);
        set => SetValue(SelectedItemsProperty, value);
    }

    protected override void OnAttached()
    {
        base.OnAttached();
        AssociatedObject.PreviewMouseLeftButtonDown += OnPreviewMouseLeftButtonDown;
    }

    protected override void OnDetaching()
    {
        AssociatedObject.PreviewMouseLeftButtonDown -= OnPreviewMouseLeftButtonDown;
        base.OnDetaching();
    }

    private void OnPreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
    {
        if (SelectedItems == null)
        {
            return;
        }

        if (e.OriginalSource is not DependencyObject source)
        {
            return;
        }

        var item = source.FindParentOfType<NodeView>();
        if (item == null || item.DataContext == null)
        {
            return;
        }

        if (Keyboard.Modifiers == ModifierKeys.Control)
        {
            // Toggle selection
            if (SelectedItems.Contains(item.DataContext))
            {
                SelectedItems.Remove((NodeViewModel)item.DataContext);
                item.IsSelected = false;
            }
            else
            {
                SelectedItems.Add((NodeViewModel)item.DataContext);
                item.IsSelected = true;
            }
        }
        else if (Keyboard.Modifiers == ModifierKeys.Shift && SelectedItems.Count > 0)
        {
            var items = GetTreeViewItemRange(source.FindParentOfType<ItemsControl>(), myLastSelectedItem, item);
            foreach (var x in items)
            {
                SelectedItems.Add((NodeViewModel)x.DataContext);
                x.IsSelected = true;
            }
        }
        else
        {
            // Single click clears and selects
            foreach (var x in SelectedItems)
            {
                x.IsSelected = false;
            }
            SelectedItems.Clear();
            SelectedItems.Add((NodeViewModel)item.DataContext);
            item.IsSelected = true;
            myLastSelectedItem = item;
        }

        // allow default behavior in parallel
        e.Handled = true;
    }

    private List<NodeView> GetTreeViewItemRange(ItemsControl parentItem, NodeView start, NodeView end)
    {
        var items = new List<NodeView>();

        for (var index = 0; index < parentItem.Items.Count; index++)
        {
            var tvItem = parentItem.ItemContainerGenerator.ContainerFromIndex(index) as NodeView;
            if (tvItem == null) continue;

            items.Add(tvItem);
        }

        var startIndex = items.IndexOf(start);
        var endIndex = items.IndexOf(end);
        var rangeStart = startIndex > endIndex || startIndex == -1 ? endIndex : startIndex;
        var rangeCount = startIndex > endIndex ? startIndex - endIndex + 1 : endIndex - startIndex + 1;

        if (startIndex == -1 && endIndex == -1)
            rangeCount = 0;

        else if (startIndex == -1 || endIndex == -1)
            rangeCount = 1;

        return rangeCount > 0 ? items.GetRange(rangeStart, rangeCount) : new List<NodeView>();
    }
}