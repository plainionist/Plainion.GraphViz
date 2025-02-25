using Plainion.GraphViz.Viewer.Abstractions.ViewModel;
using Plainion.Windows.Interactivity.DragDrop;
using Plainion.Windows.Mvvm;

namespace Plainion.GraphViz.Modules.Analysis.Clusters;

class TreeEditorViewModel : ViewModelBase, IDropable
{
    private readonly ClusterEditorViewModel myParentVM;
    private bool myShowNodeId;
    private string myFilter;
    private NodeViewModel myNodeForContextMenu;
    private string myMergeClustersCaption;

    public TreeEditorViewModel(IDomainModel model, ClusterEditorViewModel parent)
        : base(model)
    {
        System.Contract.RequiresNotNull(parent);

        myParentVM = parent;

        myShowNodeId = true;

        MergeClustersCaption = "Merge into ...";

        // "null" means root
        NewClusterCommand = new DelegateCommand(() => myParentVM.CreateNewCluster(NodeForContextMenu), () => NodeForContextMenu == null);
        DeleteNodeCommand = new DelegateCommand(myParentVM.DeleteSelectedNodes, () => NodeForContextMenu != null);
        MergeClustersCommand = new DelegateCommand(myParentVM.MergeSelectedClusters, () => myParentVM.ClusterToUpdate != null && myParentVM.SelectedNodesCount > 1);
        ExpandAllCommand = new DelegateCommand(Root.ExpandAll);
        CollapseAllCommand = new DelegateCommand(Root.CollapseAll);
    }

    public string MergeClustersCaption
    {
        get { return myMergeClustersCaption; }
        set { SetProperty(ref myMergeClustersCaption, value); }
    }

    public NodeViewModel Root => myParentVM.Root;

    public DelegateCommand NewClusterCommand { get; }
    public DelegateCommand DeleteNodeCommand { get; }
    public DelegateCommand MergeClustersCommand { get; }
    public DelegateCommand ExpandAllCommand { get; }
    public DelegateCommand CollapseAllCommand { get; }

    public bool ShowNodeId
    {
        get { return myShowNodeId; }
        set
        {
            if (SetProperty(ref myShowNodeId, value))
            {
                foreach (var clusterNode in Root.Children)
                {
                    foreach (var node in clusterNode.Children)
                    {
                        node.ShowId = myShowNodeId;
                    }
                }
            }
        }
    }

    public string Filter
    {
        get { return myFilter; }
        set
        {
            if (SetProperty(ref myFilter, value))
            {
                Root.ApplyFilter(myFilter);
            }
        }
    }

    public NodeViewModel NodeForContextMenu
    {
        get { return myNodeForContextMenu; }
        set
        {
            if (SetProperty(ref myNodeForContextMenu, value))
            {
                NewClusterCommand.RaiseCanExecuteChanged();
                DeleteNodeCommand.RaiseCanExecuteChanged();
                ExpandAllCommand.RaiseCanExecuteChanged();
                CollapseAllCommand.RaiseCanExecuteChanged();
            }
        }
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

        if (myParentVM.DropCommand.CanExecute(arg))
        {
            myParentVM.DropCommand.Execute(arg);
        }
    }
}