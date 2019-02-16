using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.Composition;
using System.Diagnostics;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows.Data;
using System.Windows.Input;
using Prism.Commands;
using Prism.Mvvm;
using Plainion.Collections;
using Plainion.GraphViz.Algorithms;
using Plainion.GraphViz.Infrastructure.ViewModel;
using Plainion.GraphViz.Model;
using Plainion.GraphViz.Presentation;
using Plainion.Prism.Mvvm;
using Plainion.Windows.Controls.Tree;
using Plainion.Windows.Interactivity.DragDrop;

namespace Plainion.GraphViz.Modules.Analysis
{
    [Export(typeof(ClusterEditorModel))]
    class ClusterEditorModel : ViewModelBase, IDropable
    {
        private string myFilter;
        private bool myFilterOnId;
        private ICollectionView myPreviewNodes;
        private NodeWithCaption mySelectedPreviewItem;
        private IGraphPresentation myPresentation;
        private DragDropBehavior myDragDropBehavior;
        private string mySelectedCluster;
        private string myAddButtonCaption;
        private IModuleChangedObserver myTransformationsObserver;
        private Dictionary<string, string> myNodeToClusterCache;
        private bool myTreeShowId;

        [ImportingConstructor]
        public ClusterEditorModel()
        {
            AddButtonCaption = "Add ...";
            AddNodesToClusterCommand = new DelegateCommand(OnAddNodesToCluster, () => SelectedCluster != null);
            MouseDownCommand = new DelegateCommand<MouseButtonEventArgs>(OnMouseDown);

            Root = new ClusterTreeNode(null);
            Root.IsDragAllowed = false;
            Root.IsDropAllowed = false;

            AddClusterCommand = new DelegateCommand<ClusterTreeNode>(OnAddCluster, n => n == Root);
            DeleteClusterCommand = new DelegateCommand<ClusterTreeNode>(OnDeleteCluster, n => n.Parent == Root);

            myDragDropBehavior = new DragDropBehavior(Root);
            DropCommand = new DelegateCommand<NodeDropRequest>(myDragDropBehavior.ApplyDrop);
        }

        public ClusterTreeNode Root { get; private set; }

        public DelegateCommand<ClusterTreeNode> AddClusterCommand { get; private set; }

        private void OnAddCluster(ClusterTreeNode parent)
        {
            // avoid many intermediate updates
            myTransformationsObserver.ModuleChanged -= OnTransformationsChanged;

            var newClusterId = Guid.NewGuid().ToString();
            var captionModule = myPresentation.GetModule<ICaptionModule>();
            captionModule.Add(new Caption(newClusterId, "<new>"));

            myPresentation.ChangeClusterAssignment(t => t.AddCluster(newClusterId));

            // start new clusters folded
            myPresentation.ChangeClusterFolding(t => t.Toggle(newClusterId));

            // update tree
            {
                var clusterNode = new ClusterTreeNode(myPresentation)
                {
                    Parent = Root,
                    Id = newClusterId,
                    Caption = captionModule.Get(newClusterId).DisplayText,
                    IsDragAllowed = false
                };
                Root.Children.Add(clusterNode);

                // register for notifications after tree is built to avoid intermediate states getting notified

                PropertyChangedEventManager.AddHandler(clusterNode, OnSelectionChanged, PropertySupport.ExtractPropertyName(() => clusterNode.IsSelected));

                // nothing ot update
                //myNodeClusterCache = null;
            }

            Root.Children.Single(n => n.Id == newClusterId).IsSelected = true;

            myTransformationsObserver.ModuleChanged += OnTransformationsChanged;
        }

        public DelegateCommand<ClusterTreeNode> DeleteClusterCommand { get; private set; }

        private void OnDeleteCluster(ClusterTreeNode clusterNode)
        {
            // avoid many intermediate updates
            myTransformationsObserver.ModuleChanged -= OnTransformationsChanged;

            myPresentation.ChangeClusterAssignment(t => t.HideCluster(clusterNode.Id));

            // update tree
            {
                Root.Children.Remove(clusterNode);

                if (clusterNode.Id == SelectedCluster)
                {
                    SelectedCluster = null;
                }

                foreach (var treeNode in clusterNode.Children)
                {
                    myNodeToClusterCache.Remove(treeNode.Id);
                }
            }

            myTransformationsObserver.ModuleChanged += OnTransformationsChanged;
        }

        public ICommand DropCommand { get; private set; }

        public ICommand MouseDownCommand { get; private set; }

        private void OnMouseDown(MouseButtonEventArgs args)
        {
            if (args.ClickCount == 2)
            {
                Filter = SelectedPreviewItem.DisplayText;
            }
        }

        public DelegateCommand AddNodesToClusterCommand { get; private set; }

        private void OnAddNodesToCluster()
        {
            // avoid many intermediate updates
            myTransformationsObserver.ModuleChanged -= OnTransformationsChanged;

            var nodes = PreviewNodes
                .Cast<NodeWithCaption>()
                .Select(n => n.Node.Id)
                .ToList();

            myPresentation.ChangeClusterAssignment(t => t.AddToCluster(nodes, SelectedCluster));

            // update tree
            {
                var clusterNode = Root.Children.Single(n => n.Id == SelectedCluster);

                var captionModule = myPresentation.GetModule<ICaptionModule>();

                var newTreeNodes = nodes
                    .Select(n => new ClusterTreeNode(myPresentation)
                    {
                        Parent = clusterNode,
                        Id = n,
                        Caption = captionModule.Get(n).DisplayText,
                        IsDropAllowed = false,
                        ShowId = TreeShowId
                    });
                clusterNode.Children.AddRange(newTreeNodes);

                // register for notifications after tree is built to avoid intermediate states getting notified

                foreach (var node in newTreeNodes)
                {
                    PropertyChangedEventManager.AddHandler(node, OnSelectionChanged, PropertySupport.ExtractPropertyName(() => node.IsSelected));
                    PropertyChangedEventManager.AddHandler(node, OnParentChanged, PropertySupport.ExtractPropertyName(() => node.Parent));

                    myNodeToClusterCache[node.Id] = clusterNode.Id;
                }
            }

            Filter = null;

            myTransformationsObserver.ModuleChanged += OnTransformationsChanged;
        }

        public string SelectedCluster
        {
            get { return mySelectedCluster; }
            set
            {
                if (SetProperty(ref mySelectedCluster, value))
                {
                    var captionModule = myPresentation.GetModule<ICaptionModule>();
                    AddButtonCaption = SelectedCluster != null ? "Add to '" + captionModule.Get(mySelectedCluster).DisplayText + "'" : "Add ...";

                    AddNodesToClusterCommand.RaiseCanExecuteChanged();
                }
            }
        }

        public string AddButtonCaption
        {
            get { return myAddButtonCaption; }
            set { SetProperty(ref myAddButtonCaption, value); }
        }

        public NodeWithCaption SelectedPreviewItem
        {
            get { return mySelectedPreviewItem; }
            set { SetProperty(ref mySelectedPreviewItem, value); }
        }

        protected override void OnModelPropertyChanged(string propertyName)
        {
            if (propertyName == "Presentation")
            {
                if (myPresentation == Model.Presentation)
                {
                    return;
                }

                if (myTransformationsObserver != null)
                {
                    myTransformationsObserver.ModuleChanged -= OnTransformationsChanged;
                    myTransformationsObserver.Dispose();
                }

                myPresentation = Model.Presentation;

                // first build tree - master for preview
                BuildTree();

                // rebuild preview
                myPreviewNodes = null;
                if (Filter == null)
                {
                    PreviewNodes.Refresh();
                }
                else
                {
                    Filter = null;
                }

                // register for updates only AFTER tree is built up completely to avoid getting notified by the built up process
                var transformationModule = Model.Presentation.GetModule<ITransformationModule>();
                myTransformationsObserver = transformationModule.CreateObserver();
                myTransformationsObserver.ModuleChanged += OnTransformationsChanged;
            }
        }

        private void BuildTree()
        {
            using (new Profile("BuildTree"))
            {
                Root.Children.Clear();

                SelectedCluster = null;

                var transformationModule = myPresentation.GetModule<ITransformationModule>();
                var captionModule = myPresentation.GetModule<ICaptionModule>();

                foreach (var cluster in transformationModule.Graph.Clusters.OrderBy(c => c.Id))
                {
                    var clusterNode = new ClusterTreeNode(myPresentation)
                    {
                        Parent = Root,
                        Id = cluster.Id,
                        Caption = captionModule.Get(cluster.Id).DisplayText,
                        IsDragAllowed = false
                    };
                    Root.Children.Add(clusterNode);

                    // we do not want to see the pseudo node added for folding but the full expanded list of nodes of this cluster
                    var folding = transformationModule.Items
                        .OfType<ClusterFoldingTransformation>()
                        .SingleOrDefault(f => f.Clusters.Contains(cluster.Id));

                    var nodes = folding == null ? cluster.Nodes : folding.GetNodes(cluster.Id);

                    clusterNode.Children.AddRange(nodes
                        .Select(n => new ClusterTreeNode(myPresentation)
                        {
                            Parent = clusterNode,
                            Id = n.Id,
                            Caption = captionModule.Get(n.Id).DisplayText,
                            IsDropAllowed = false,
                            ShowId = TreeShowId
                        }));
                }

                // register for notifications after tree is built to avoid intermediate states getting notified

                foreach (ClusterTreeNode cluster in Root.Children)
                {
                    PropertyChangedEventManager.AddHandler(cluster, OnSelectionChanged, PropertySupport.ExtractPropertyName(() => cluster.IsSelected));

                    foreach (ClusterTreeNode node in cluster.Children)
                    {
                        PropertyChangedEventManager.AddHandler(node, OnSelectionChanged, PropertySupport.ExtractPropertyName(() => node.IsSelected));
                        PropertyChangedEventManager.AddHandler(node, OnParentChanged, PropertySupport.ExtractPropertyName(() => node.Parent));
                    }
                }

                myNodeToClusterCache = null;
            }
        }

        private void OnParentChanged(object sender, PropertyChangedEventArgs e)
        {
            var node = (ClusterTreeNode)sender;

            myPresentation.ChangeClusterAssignment(t => t.AddToCluster(node.Id, ((ClusterTreeNode)node.Parent).Id));
        }

        private void OnSelectionChanged(object sender, PropertyChangedEventArgs e)
        {
            var selectedCluster = Root.Children.FirstOrDefault(n => n.IsSelected);
            if (selectedCluster == null)
            {
                var selectedNode = Root.Children
                    .SelectMany(n => n.Children)
                    .FirstOrDefault(n => n.IsSelected);

                if (selectedNode != null)
                {
                    selectedCluster = (ClusterTreeNode)selectedNode.Parent;
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

        private void OnTransformationsChanged(object sender, EventArgs e)
        {
            BuildTree();
            PreviewNodes.Refresh();
        }

        public bool TreeShowId
        {
            get { return myTreeShowId; }
            set
            {
                if (SetProperty(ref myTreeShowId, value))
                {
                    foreach (var clusterNode in Root.Children)
                    {
                        foreach (var node in clusterNode.Children)
                        {
                            node.ShowId = myTreeShowId;
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

            // we do not look into model because handlign the ITransformationModule esp. with folding
            // is too complex. anyhow the "model" for the preview can also be the tree in this case.
            if (myNodeToClusterCache == null)
            {
                Debug.WriteLine("Rebuilding NodeClusterCache");

                myNodeToClusterCache = new Dictionary<string, string>();

                foreach (ClusterTreeNode cluster in Root.Children)
                {
                    foreach (ClusterTreeNode treeNode in cluster.Children)
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
                return new Plainion.Text.Wildcard(pattern, RegexOptions.IgnoreCase).IsMatch(node.DisplayText);
            }
            catch
            {
                SetError(ValidationFailure.Error, "Filter");
                return true;
            }
        }

        string IDropable.DataFormat
        {
            get { return typeof(NodeItem).FullName; }
        }

        bool IDropable.IsDropAllowed(object data, DropLocation location)
        {
            return true;
        }

        void IDropable.Drop(object data, DropLocation location)
        {
            var droppedElement = data as NodeItem;
            if (droppedElement == null)
            {
                return;
            }

            var nodeId = ((ClusterTreeNode)droppedElement.DataContext).Id;

            myPresentation.ChangeClusterAssignment(t => t.RemoveFromClusters(nodeId));
        }
    }
}
