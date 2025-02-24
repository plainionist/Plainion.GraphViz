using System;
using System.Linq;
using System.Windows;
using Plainion.GraphViz.Presentation;
using Plainion.GraphViz.Viewer.Abstractions.ViewModel;
using Plainion.Windows.Mvvm;

namespace Plainion.GraphViz.Modules.Analysis.Clusters;

class TreeEditorViewModel : ViewModelBase
{
    private readonly ClusterEditorModel myParentVM;
    private bool myShowNodeId;
    private string mySelectedCluster;
    private string myFilter;
    private IGraphPresentation myPresentation;

    public TreeEditorViewModel(IDomainModel model, ClusterEditorModel parent)
        : base(model)
    {
        System.Contract.RequiresNotNull(parent);

        myParentVM = parent;

        myShowNodeId = true;

        ExpandAllCommand = new DelegateCommand(() => myParentVM.Root.ExpandAll());
        CollapseAllCommand = new DelegateCommand(() => myParentVM.Root.CollapseAll());
    }

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
            var selectedNode = myParentVM.Root.Children
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

    public DelegateCommand ExpandAllCommand { get; }
    public DelegateCommand CollapseAllCommand { get; }

    public bool ShowNodeId
    {
        get { return myShowNodeId; }
        set
        {
            if (SetProperty(ref myShowNodeId, value))
            {
                foreach (var clusterNode in myParentVM.Root.Children)
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
                myParentVM.Root.ApplyFilter(myFilter);
            }
        }
    }
}