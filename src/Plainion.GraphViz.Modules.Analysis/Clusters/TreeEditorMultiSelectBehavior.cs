using System.Collections;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Microsoft.Xaml.Behaviors;

namespace Plainion.GraphViz.Modules.Analysis.Clusters;

public class TreeEditorMultiSelectBehavior : Behavior<TreeView>
{
    public static readonly DependencyProperty SelectedItemsProperty =
        DependencyProperty.Register(nameof(SelectedItems), typeof(IList), typeof(TreeEditorMultiSelectBehavior),
            new PropertyMetadata(null));

    public IList SelectedItems
    {
        get => (IList)GetValue(SelectedItemsProperty);
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

        var item = source.FindParentOfType<TreeViewItem>();
        if (item == null || item.DataContext == null)
        {
            return;
        }

        if (Keyboard.Modifiers == ModifierKeys.Control)
        {
            // Toggle selection
            if (SelectedItems.Contains(item.DataContext))
            {
                SelectedItems.Remove(item.DataContext);
            }
            else
            {
                SelectedItems.Add(item.DataContext);
            }
        }
        else if (Keyboard.Modifiers == ModifierKeys.Shift && SelectedItems.Count > 0)
        {
            // Range selection (optional, requires more logic for hierarchy)
            // For simplicity, we'll skip full range logic here
        }
        else
        {
            // Single click clears and selects
            SelectedItems.Clear();
            SelectedItems.Add(item.DataContext);
        }

        // allow default behavior in parallel
        // e.Handled = true;
    }
}
