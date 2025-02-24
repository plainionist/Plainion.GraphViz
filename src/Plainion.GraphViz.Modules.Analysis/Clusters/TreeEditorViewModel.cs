﻿using System.Linq;
using Plainion.GraphViz.Presentation;
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
    private IGraphPresentation myPresentation;

    public TreeEditorViewModel(IDomainModel model, ClusterEditorViewModel parent)
        : base(model)
    {
        System.Contract.RequiresNotNull(parent);

        myParentVM = parent;

        myShowNodeId = true;

        // "null" means root
        NewClusterCommand = new DelegateCommand(() => myParentVM.CreateNewCluster(NodeForContextMenu), () => NodeForContextMenu == null);
        DeleteNodeCommand = new DelegateCommand(OnDeleteNotes, () => NodeForContextMenu != null);
        ExpandAllCommand = new DelegateCommand(Root.ExpandAll);
        CollapseAllCommand = new DelegateCommand(Root.CollapseAll);
    }

    private void OnDeleteNotes()
    {
        foreach (var cluster in Root.Children.ToList())
        {
            if (cluster.IsSelected)
            {
                myParentVM.DeleteNode(cluster);
            }
            else
            {
                foreach (var node in cluster.Children.Where(x => x.IsSelected).ToList())
                {
                    myParentVM.DeleteNode(node);
                }
            }
        }
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