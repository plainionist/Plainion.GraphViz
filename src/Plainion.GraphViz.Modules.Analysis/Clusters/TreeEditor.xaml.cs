using System.Collections.Specialized;
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

    public static DependencyProperty FilterLabelProperty = DependencyProperty.Register("FilterLabel", typeof(string), typeof(TreeEditor),
        new FrameworkPropertyMetadata(null));

    public string FilterLabel
    {
        get { return (string)GetValue(FilterLabelProperty); }
        set { SetValue(FilterLabelProperty, value); }
    }

    public static DependencyProperty RootProperty = DependencyProperty.Register("Root", typeof(NodeViewModel), typeof(TreeEditor),
        new FrameworkPropertyMetadata(null, OnRootChanged));

    private static void OnRootChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        var self = (TreeEditor)d;
        self.OnRootChanged();
    }

    private void OnRootChanged()
    {
        myTree.Root = Root;

        if (Root != null)
        {
            CollectionChangedEventManager.AddHandler(Root.Children, OnRootChildrenChanged);
        }
    }

    private void OnRootChildrenChanged(object sender, NotifyCollectionChangedEventArgs e)
    {
        // looks like tree was rebuilt -> reapply filter
        OnFilterChanged();
    }

    public NodeViewModel Root
    {
        get { return (NodeViewModel)GetValue(RootProperty); }
        set { SetValue(RootProperty, value); }
    }

    public static DependencyProperty FilterProperty = DependencyProperty.Register("Filter", typeof(string), typeof(TreeEditor),
        new FrameworkPropertyMetadata(OnFilterChanged));

    private static void OnFilterChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (e.NewValue == e.OldValue)
        {
            return;
        }

        ((TreeEditor)d).OnFilterChanged();
    }

    private void OnFilterChanged()
    {
        Root?.ApplyFilter(Filter);
    }

    public string Filter
    {
        get { return (string)GetValue(FilterProperty); }
        set { SetValue(FilterProperty, value); }
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
