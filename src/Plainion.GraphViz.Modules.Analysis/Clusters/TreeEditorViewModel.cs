using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Plainion.GraphViz.Presentation;
using Plainion.GraphViz.Viewer.Abstractions.ViewModel;
using Plainion.Windows.Interactivity.DragDrop;
using Plainion.Windows.Mvvm;

namespace Plainion.GraphViz.Modules.Analysis.Clusters;

class TreeEditorViewModel : ViewModelBase, IDropable
{
    private readonly ClusterEditorViewModel myParentVM;
    private bool myShowNodeId;
    private string mySelectedCluster;
    private string myFilter;
    private NodeViewModel myNodeForContextMenu;
    private IGraphPresentation myPresentation;

    public TreeEditorViewModel(IDomainModel model, ClusterEditorViewModel parent)
        : base(model)
    {
        System.Contract.RequiresNotNull(parent);

        myParentVM = parent;

        myShowNodeId = true;

        NewClusterCommand = new DelegateCommand(() => myParentVM.OnNewCluster(NodeForContextMenu), () => NodeForContextMenu == Root);
        DeleteNodeCommand = new DelegateCommand(() => myParentVM.OnDeleteNode(NodeForContextMenu), () => NodeForContextMenu != Root);
        ExpandAllCommand = new DelegateCommand(Root.ExpandAll);
        CollapseAllCommand = new DelegateCommand(Root.CollapseAll);
    }

    public NodeViewModel Root => myParentVM.Root;

    protected override void OnPresentationChanged()
    {
        if (myPresentation == Model.Presentation)
        {
            return;
        }

        myPresentation = Model.Presentation;
    }

    internal void SelectCluster(NodeViewModel selectedCluster)
    {
        if (selectedCluster == null)
        {
            var selectedNode = Root.Children
                .SelectMany(n => n.Children)
                .FirstOrDefault(n => n.IsSelected);

            if (selectedNode != null)
            {
                selectedCluster = selectedNode.Parent;
            }
        }

        if (selectedCluster == null)
        {
            SelectedCluster = null;
        }
        else
        {
            SelectedCluster = selectedCluster.Id;
        }
    }

    public DelegateCommand NewClusterCommand { get; }
    public DelegateCommand DeleteNodeCommand { get; }
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

    public string SelectedCluster
    {
        get { return mySelectedCluster; }
        set
        {
            if (SetProperty(ref mySelectedCluster, value))
            {
                myParentVM.OnClusterSelected(mySelectedCluster);
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