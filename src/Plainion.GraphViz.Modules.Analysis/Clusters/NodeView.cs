using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using Plainion.Windows.Interactivity.DragDrop;
using Plainion.Windows.Mvvm;

namespace Plainion.GraphViz.Modules.Analysis.Clusters;

public class NodeView : TreeViewItem, IDropable, IDragable
{
    internal NodeView()
    {
        if (!DesignerProperties.GetIsInDesignMode(this))
        {
            EditCommand = new DelegateCommand(() => IsInEditMode = true, () =>
            {
                var expr = GetBindingExpression(TextProperty);
                return expr != null && expr.ParentBinding.Mode == BindingMode.TwoWay;
            });

            Loaded += OnLoaded;
        }
    }

    private void OnLoaded(object sender, RoutedEventArgs e)
    {
        Loaded -= OnLoaded;

        if (BindingOperations.GetBindingExpression(this, FormattedTextProperty) == null
            && BindingOperations.GetMultiBindingExpression(this, FormattedTextProperty) == null)
        {
            SetBinding(FormattedTextProperty, new Binding { Path = new PropertyPath("Text"), Source = this });
        }
    }

    protected override DependencyObject GetContainerForItemOverride() => new NodeView();

    protected override bool IsItemItsOwnContainerOverride(object item) => item is NodeView;

    public static DependencyProperty TextProperty = DependencyProperty.Register("Text", typeof(string), typeof(NodeView), new FrameworkPropertyMetadata(null));

    public string Text
    {
        get { return (string)GetValue(TextProperty); }
        set { SetValue(TextProperty, value); }
    }

    public static DependencyProperty FormattedTextProperty = DependencyProperty.Register("FormattedText", typeof(string), typeof(NodeView), new FrameworkPropertyMetadata(null));

    public string FormattedText
    {
        get { return (string)GetValue(FormattedTextProperty); }
        set { SetValue(FormattedTextProperty, value); }
    }

    public static DependencyProperty IsInEditModeProperty = DependencyProperty.Register("IsInEditMode", typeof(bool), typeof(NodeView), new FrameworkPropertyMetadata(false));

    public bool IsInEditMode
    {
        get { return (bool)GetValue(IsInEditModeProperty); }
        set { SetValue(IsInEditModeProperty, value); }
    }

    public static DependencyProperty IsFilteredOutProperty = DependencyProperty.Register("IsFilteredOut", typeof(bool), typeof(NodeView), new FrameworkPropertyMetadata(false, OnIsFilteredOutChanged));

    private static void OnIsFilteredOutChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        var self = (NodeView)d;
        self.Visibility = self.IsFilteredOut ? Visibility.Collapsed : Visibility.Visible;
    }

    public bool IsFilteredOut
    {
        get { return (bool)GetValue(IsFilteredOutProperty); }
        set { SetValue(IsFilteredOutProperty, value); ((NodeViewModel)DataContext).IsFilteredOut = value; }
    }

    string IDropable.DataFormat
    {
        get { return typeof(NodeView).FullName; }
    }

    bool IDropable.IsDropAllowed(object data, DropLocation location) => data is NodeView;

    void IDropable.Drop(object data, DropLocation _)
    {
        if (data is not NodeView droppedElement)
        {
            return;
        }

        if (object.ReferenceEquals(droppedElement, this))
        {
            //if dragged and dropped yourself, don't need to do anything
            return;
        }

        var arg = new NodeDropRequest((NodeViewModel)droppedElement.DataContext, (NodeViewModel)DataContext);

        var editor = this.FindParentOfType<TreeEditor>();
        if (editor.DropCommand != null && editor.DropCommand.CanExecute(arg))
        {
            editor.DropCommand.Execute(arg);
        }
    }

    Type IDragable.DataType => typeof(NodeView);

    public ICommand EditCommand { get; }

    protected override void OnPreviewKeyDown(KeyEventArgs e)
    {
        // we didnt managed to handle InputBindings in xaml for these shortcuts without breaking the 
        // keyboard navigation within the TreeView

        if (e.Key == Key.F2 && IsSelected)
        {
            if (EditCommand.CanExecute(null))
            {
                EditCommand.Execute(null);
            }

            e.Handled = true;
        }
        else
        {
            base.OnPreviewKeyDown(e);
        }
    }
}
