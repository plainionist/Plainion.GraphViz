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

        CommandBindings.Add(new CommandBinding(ExpandAll, (sender, e) => OnExpandAll((ClusterTreeNode)e.Parameter)));
        CommandBindings.Add(new CommandBinding(CollapseAll, (sender, e) => OnCollapseAll((ClusterTreeNode)e.Parameter)));
    }

    public static readonly RoutedCommand ExpandAll = new();
    public static readonly RoutedCommand CollapseAll = new();

    private void OnExpandAll(ClusterTreeNode node)
    {
        if (node == null)
        {
            node = Root;
        }

        var nodeState = myTree.StateContainer.GetOrCreate(node);

        nodeState.ExpandAll();
    }

    private void OnCollapseAll(ClusterTreeNode node)
    {
        if (node == null)
        {
            node = Root;
        }

        var nodeState = myTree.StateContainer.GetOrCreate(node);

        nodeState.CollapseAll();
    }

    public static DependencyProperty FilterLabelProperty = DependencyProperty.Register("FilterLabel", typeof(string), typeof(TreeEditor),
        new FrameworkPropertyMetadata(null));

    public string FilterLabel
    {
        get { return (string)GetValue(FilterLabelProperty); }
        set { SetValue(FilterLabelProperty, value); }
    }

    public static DependencyProperty RootProperty = DependencyProperty.Register("Root", typeof(ClusterTreeNode), typeof(TreeEditor),
        new FrameworkPropertyMetadata(null, OnRootChanged));

    private static void OnRootChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        var self = (TreeEditor)d;
        self.OnRootChanged();
    }

    private void OnRootChanged()
    {
        myTree.StateContainer.DataContext = Root;

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

    public ClusterTreeNode Root
    {
        get { return (ClusterTreeNode)GetValue(RootProperty); }
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
        if (Root == null)
        {
            return;
        }

        myTree.StateContainer.GetRoot().ApplyFilter(Filter);
    }

    public string Filter
    {
        get { return (string)GetValue(FilterProperty); }
        set { SetValue(FilterProperty, value); }
    }

    string IDropable.DataFormat
    {
        get { return typeof(NodeItem).FullName; }
    }

    bool IDropable.IsDropAllowed(object data, DropLocation location)
    {
        if (Root == null)
        {
            return false;
        }

        return myTree.StateContainer.GetOrCreate(Root).IsDropAllowed(location);
    }

    void IDropable.Drop(object data, DropLocation location)
    {
        var droppedElement = data as NodeItem;
        if (droppedElement == null)
        {
            return;
        }

        var arg = new NodeDropRequest
        {
            DroppedNode = droppedElement.State.DataContext,
            DropTarget = Root,
            Location = location
        };

        if (DropCommand != null && DropCommand.CanExecute(arg))
        {
            DropCommand.Execute(arg);
        }
    }

    public static DependencyProperty ExpandAllCommandProperty = DependencyProperty.Register("ExpandAllCommand", typeof(ICommand), typeof(TreeEditor),
        new FrameworkPropertyMetadata(ExpandAll));

    public ICommand ExpandAllCommand
    {
        get { return (ICommand)GetValue(ExpandAllCommandProperty); }
        set { SetValue(ExpandAllCommandProperty, value); }
    }

    public static DependencyProperty CollapseAllCommandProperty = DependencyProperty.Register("CollapseAllCommand", typeof(ICommand), typeof(TreeEditor),
        new FrameworkPropertyMetadata(CollapseAll));

    public ICommand CollapseAllCommand
    {
        get { return (ICommand)GetValue(CollapseAllCommandProperty); }
        set { SetValue(CollapseAllCommandProperty, value); }
    }

    public static DependencyProperty CreateChildCommandProperty = DependencyProperty.Register("CreateChildCommand", typeof(ICommand), typeof(TreeEditor),
        new FrameworkPropertyMetadata(null));

    public ICommand CreateChildCommand
    {
        get { return (ICommand)GetValue(CreateChildCommandProperty); }
        set { SetValue(CreateChildCommandProperty, value); }
    }

    public static DependencyProperty DeleteCommandProperty = DependencyProperty.Register("DeleteCommand", typeof(ICommand), typeof(TreeEditor),
        new FrameworkPropertyMetadata(null));

    public ICommand DeleteCommand
    {
        get { return (ICommand)GetValue(DeleteCommandProperty); }
        set { SetValue(DeleteCommandProperty, value); }
    }

    /// <summary>
    /// Parameter will be of type <see cref="NodeDropRequest"/>.
    /// </summary>
    public static DependencyProperty DropCommandProperty = DependencyProperty.Register("DropCommand", typeof(ICommand), typeof(TreeEditor),
        new FrameworkPropertyMetadata(null));

    public ICommand DropCommand
    {
        get { return (ICommand)GetValue(DropCommandProperty); }
        set { SetValue(DropCommandProperty, value); }
    }
}
