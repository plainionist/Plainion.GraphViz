using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows.Data;
using System.Windows.Input;
using Plainion.Collections;
using Plainion.GraphViz.Presentation;
using Plainion.GraphViz.Viewer.Abstractions.ViewModel;
using Plainion.Prism.Mvvm;
using Plainion.Windows.Interactivity.DragDrop;
using Prism.Commands;
using Prism.Mvvm;

namespace Plainion.GraphViz.Modules.Analysis.Clusters
{
    class ClusterEditorModel : ViewModelBase, IDropable
    {
        private string myFilter;
        private bool myFilterOnId;
        private ICollectionView myPreviewNodes;
        private NodeWithCaption mySelectedPreviewItem;
        private IGraphPresentation myPresentation;
        private string mySelectedCluster;
        private string myAddButtonCaption;
        private IModuleChangedObserver myTransformationsObserver;
        private Dictionary<string, string> myNodeToClusterCache;
        private bool myTreeShowId;

        public ClusterEditorModel(IDomainModel model)
            : base(model)
        {
            Root = new NodeViewModel(null, null, NodeType.Root);

            AddButtonCaption = "Add ...";
            AddNodesToClusterCommand = new DelegateCommand(OnAddNodesToCluster, () => SelectedCluster != null);
            MouseDownCommand = new DelegateCommand<MouseButtonEventArgs>(OnMouseDown);

            NewClusterCommand = new DelegateCommand<NodeViewModel>(OnNewCluster, n => n == Root);
            DeleteNodeCommand = new DelegateCommand<NodeViewModel>(OnDeleteNode, n => n != Root);
            DropCommand = new DelegateCommand<NodeDropRequest>(OnDrop);

            ExpandAllCommand = new DelegateCommand(() => Root.ExpandAll());
            CollapseAllCommand = new DelegateCommand(() => Root.CollapseAll());

            myFilterOnId = true;
            myTreeShowId = true;
        }

        public NodeViewModel Root { get; }

        public DelegateCommand ExpandAllCommand { get; }
        public DelegateCommand CollapseAllCommand { get; }

        public DelegateCommand<NodeViewModel> NewClusterCommand { get; }

        private void OnNewCluster(NodeViewModel parent)
        {
            // avoid many intermediate updates
            myTransformationsObserver.ModuleChanged -= OnTransformationsChanged;

            var newClusterId = Guid.NewGuid().ToString();
            var captionModule = myPresentation.GetModule<ICaptionModule>();
            captionModule.Add(new Caption(newClusterId, "<new>"));

            myPresentation.DynamicClusters().AddCluster(newClusterId);

            // start new clusters folded
            myPresentation.ClusterFolding().Toggle(newClusterId);

            // update tree
            {
                var clusterNode = new NodeViewModel(myPresentation, newClusterId, NodeType.Cluster)
                {
                    Parent = Root,
                    Caption = captionModule.Get(newClusterId).DisplayText,
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

        public DelegateCommand<NodeViewModel> DeleteNodeCommand { get; }

        private void OnDeleteNode(NodeViewModel node)
        {
            if (node.Type == NodeType.Cluster)
            {
                // avoid many intermediate updates
                myTransformationsObserver.ModuleChanged -= OnTransformationsChanged;

                myPresentation.DynamicClusters().HideCluster(node.Id);

                // update tree
                {
                    // the tree might have been rebuilt - we have to search by id
                    Root.Children.Remove(Root.Children.Single(x => x.Id == node.Id));

                    if (node.Id == SelectedCluster)
                    {
                        SelectedCluster = null;
                    }

                    foreach (var treeNode in node.Children)
                    {
                        myNodeToClusterCache.Remove(treeNode.Id);
                    }
                }

                myTransformationsObserver.ModuleChanged += OnTransformationsChanged;
            }
            else
            {
                // remove node
                myPresentation.DynamicClusters().RemoveFromClusters(node.Id);
            }

            PreviewNodes.Refresh();
        }

        public ICommand DropCommand { get; }

        private void OnDrop(NodeDropRequest request)
        {
            var targetCluster = request.DropTarget.Type == NodeType.Node ? request.DropTarget.Parent : request.DropTarget;
            var droppedNodes = request.DroppedNode.Type == NodeType.Cluster ? request.DroppedNode.Children : [request.DroppedNode];

            // need a copy as we modify the collection in the loop
            foreach (var droppedNode in droppedNodes.ToList())
            {
                droppedNode.Parent.Children.Remove(droppedNode);

                targetCluster.Children.Add(droppedNode);

                droppedNode.Parent = targetCluster;
            }

            if (request.DroppedNode.Type == NodeType.Cluster)
            {
                OnDeleteNode(request.DroppedNode);
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

        public DelegateCommand AddNodesToClusterCommand { get; }

        private void OnAddNodesToCluster()
        {
            // avoid many intermediate updates
            myTransformationsObserver.ModuleChanged -= OnTransformationsChanged;

            var nodes = PreviewNodes
                .Cast<NodeWithCaption>()
                .Select(n => n.Node.Id)
                .ToList();

            myPresentation.DynamicClusters().AddToCluster(nodes, SelectedCluster);

            // update tree
            {
                var clusterNode = Root.Children.Single(n => n.Id == SelectedCluster);

                var captionModule = myPresentation.GetModule<ICaptionModule>();

                var newTreeNodes = nodes
                    .Select(n => new NodeViewModel(myPresentation, n, NodeType.Node)
                    {
                        Parent = clusterNode,
                        Caption = captionModule.Get(n).DisplayText,
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

            myTransformationsObserver.ModuleChanged += OnTransformationsChanged;

            Filter = null;
            PreviewNodes.Refresh();
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

        protected override void OnPresentationChanged()
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

        private void BuildTree()
        {
            using (new Profile("BuildTree"))
            {
                var expandedClusterIds = Root.Children
                    .Where(x => x.IsExpanded)
                    .Select(x => x.Id)
                    .ToHashSet();

                Root.Children.Clear();

                SelectedCluster = null;

                var transformationModule = myPresentation.GetModule<ITransformationModule>();
                var captionModule = myPresentation.GetModule<ICaptionModule>();
                var clusterFolding = myPresentation.ClusterFolding();

                foreach (var cluster in transformationModule.Graph.Clusters.OrderBy(c => c.Id))
                {
                    var clusterNode = new NodeViewModel(myPresentation, cluster.Id, NodeType.Cluster)
                    {
                        Parent = Root,
                        Caption = captionModule.Get(cluster.Id).DisplayText,
                    };
                    Root.Children.Add(clusterNode);

                    // we do not want to see the pseudo node added for folding but the full expanded list of nodes of this cluster
                    var nodes = clusterFolding == null ? cluster.Nodes : clusterFolding.GetNodes(cluster.Id);

                    clusterNode.Children.AddRange(nodes
                        .Select(n => new NodeViewModel(myPresentation, n.Id, NodeType.Node)
                        {
                            Parent = clusterNode,
                            Caption = captionModule.Get(n.Id).DisplayText,
                            ShowId = TreeShowId
                        }));
                }

                // register for notifications after tree is built to avoid intermediate states getting notified

                foreach (var cluster in Root.Children)
                {
                    PropertyChangedEventManager.AddHandler(cluster, OnSelectionChanged, PropertySupport.ExtractPropertyName(() => cluster.IsSelected));

                    foreach (var node in cluster.Children)
                    {
                        PropertyChangedEventManager.AddHandler(node, OnSelectionChanged, PropertySupport.ExtractPropertyName(() => node.IsSelected));
                        PropertyChangedEventManager.AddHandler(node, OnParentChanged, PropertySupport.ExtractPropertyName(() => node.Parent));
                    }
                }

                // make sure expanded clusters are expanded after node deletion and Drag&Drop
                foreach (var node in Root.Children.Where(x => expandedClusterIds.Contains(x.Id)))
                {
                    node.IsExpanded = true;
                }

                myNodeToClusterCache = null;
            }
        }

        private void OnParentChanged(object sender, PropertyChangedEventArgs e)
        {
            var node = (NodeViewModel)sender;

            myPresentation.DynamicClusters().AddToCluster(node.Id, node.Parent.Id);
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

                foreach (NodeViewModel cluster in Root.Children)
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

            OnDeleteNode((NodeViewModel)droppedElement.DataContext);
        }
    }
}
