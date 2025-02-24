using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows.Data;
using System.Windows.Input;
using Plainion.GraphViz.Presentation;
using Plainion.GraphViz.Viewer.Abstractions.ViewModel;
using Plainion.Prism.Mvvm;
using Plainion.Windows.Interactivity.DragDrop;
using Plainion.Windows.Mvvm;

namespace Plainion.GraphViz.Modules.Analysis.Clusters;

internal class PreviewViewModel : ViewModelBase, IDropable
{
    private readonly ClusterEditorViewModel myParentVM;
    private NodeWithCaption mySelectedPreviewItem;
    private string myFilter;
    private bool myFilterOnId;
    private ICollectionView myPreviewNodes;
    private IGraphPresentation myPresentation;
    private Dictionary<string, string> myNodeToClusterCache;
    private string myAddButtonCaption;

    public PreviewViewModel(IDomainModel model, ClusterEditorViewModel parent)
        : base(model)
    {
        System.Contract.RequiresNotNull(parent);

        myParentVM = parent;

        AddButtonCaption = "Add ...";
        MouseDownCommand = new DelegateCommand<MouseButtonEventArgs>(OnMouseDown);

        myFilterOnId = true;
    }

    protected override void OnPresentationChanged()
    {
        if (myPresentation == Model.Presentation)
        {
            return;
        }

        myPresentation = Model.Presentation;

        myPreviewNodes = null;
        if (Filter == null)
        {
            PreviewNodes.Refresh();
        }
        else
        {
            Filter = null;
        }
        myNodeToClusterCache = null;
    }

    public string AddButtonCaption
    {
        get { return myAddButtonCaption; }
        set { SetProperty(ref myAddButtonCaption, value); }
    }

    public string Filter
    {
        get { return myFilter; }
        set
        {
            if (SetProperty(ref myFilter, value))
            {
                ClearErrors();
                PreviewNodes.Refresh();
            }
        }
    }

    public bool FilterOnId
    {
        get { return myFilterOnId; }
        set
        {
            if (SetProperty(ref myFilterOnId, value))
            {
                ClearErrors();
                PreviewNodes.Refresh();
            }
        }
    }

    public ICollectionView PreviewNodes
    {
        get
        {
            if (myPreviewNodes == null && myPresentation != null)
            {
                var captionModule = myPresentation.GetPropertySetFor<Caption>();

                var nodes = myPresentation.Graph.Nodes
                    .Select(n => new NodeWithCaption(n, myFilterOnId ? n.Id : captionModule.Get(n.Id).DisplayText));

                myPreviewNodes = CollectionViewSource.GetDefaultView(nodes);
                myPreviewNodes.Filter = FilterPreview;
                myPreviewNodes.SortDescriptions.Add(new SortDescription("DisplayText", ListSortDirection.Ascending));

                RaisePropertyChanged(nameof(PreviewNodes));
            }
            return myPreviewNodes;
        }
    }

    private bool FilterPreview(object item)
    {
        if (GetErrors("Filters").OfType<object>().Any())
        {
            return true;
        }

        var node = (NodeWithCaption)item;

        // we do not look into model because handling the ITransformationModule esp. with folding
        // is too complex. anyhow the "model" for the preview can also be the tree in this case.
        if (myNodeToClusterCache == null)
        {
            Debug.WriteLine("Rebuilding NodeClusterCache");

            myNodeToClusterCache = [];

            foreach (NodeViewModel cluster in myParentVM.Root.Children)
            {
                foreach (NodeViewModel treeNode in cluster.Children)
                {
                    myNodeToClusterCache[treeNode.Id] = cluster.Id;
                }
            }
        }

        if (myNodeToClusterCache.ContainsKey(node.Node.Id))
        {
            return false;
        }

        if (string.IsNullOrEmpty(Filter))
        {
            return true;
        }

        var pattern = Filter;

        if (!pattern.Contains('*'))
        {
            pattern = "*" + pattern + "*";
        }

        try
        {
            return new Text.Wildcard(pattern, RegexOptions.IgnoreCase).IsMatch(node.DisplayText);
        }
        catch
        {
            SetError(ValidationFailure.Error, "Filter");
            return true;
        }
    }

    public ICommand MouseDownCommand { get; }

    private void OnMouseDown(MouseButtonEventArgs args)
    {
        if (args.ClickCount == 2)
        {
            Filter = SelectedPreviewItem.DisplayText;
        }
    }

    public NodeWithCaption SelectedPreviewItem
    {
        get { return mySelectedPreviewItem; }
        set { SetProperty(ref mySelectedPreviewItem, value); }
    }

    internal void OnClusterDeleted(NodeViewModel node)
    {
        foreach (var treeNode in node.Children)
        {
            myNodeToClusterCache?.Remove(treeNode.Id);
        }

        PreviewNodes.Refresh();
    }

    internal void OnNodeAddedToCluster(NodeViewModel node, NodeViewModel clusterNode)
    {
        if (myNodeToClusterCache != null)
        {
            myNodeToClusterCache[node.Id] = clusterNode.Id;
        }
    }

    internal void OnTransformationsChanged()
    {
        myNodeToClusterCache = null;
        PreviewNodes.Refresh();
    }

    internal void OnNodeRemovedFromCluster(NodeViewModel node)
    {
        PreviewNodes.Refresh();
    }

    string IDropable.DataFormat
    {
        get { return typeof(NodeView).FullName; }
    }

    bool IDropable.IsDropAllowed(object data, DropLocation location) => true;

    // move node out from tree into preview
    void IDropable.Drop(object data, DropLocation location)
    {
        if (data is not NodeView droppedElement)
        {
            return;
        }

        myParentVM.OnDeleteNode((NodeViewModel)droppedElement.DataContext);
    }
}