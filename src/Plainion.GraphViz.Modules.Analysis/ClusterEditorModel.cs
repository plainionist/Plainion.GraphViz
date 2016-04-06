using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.Composition;
using System.Diagnostics;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows.Data;
using System.Windows.Input;
using Microsoft.Practices.Prism.Commands;
using Microsoft.Practices.Prism.Mvvm;
using Plainion.Collections;
using Plainion.GraphViz.Algorithms;
using Plainion.GraphViz.Infrastructure.ViewModel;
using Plainion.GraphViz.Presentation;
using Plainion.Prism.Mvvm;
using Plainion.Windows.Controls.Tree;

namespace Plainion.GraphViz.Modules.Analysis
{
    [Export(typeof(ClusterEditorModel))]
    class ClusterEditorModel : ViewModelBase
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
        private Dictionary<string, string> myNodeClusterCache;
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

            AddClusterCommand = new DelegateCommand<ClusterTreeNode>(OnAddCluster,
                // only allow adding clusters
                p => p == Root);
            DeleteClusterCommand = new DelegateCommand<ClusterTreeNode>(OnDeleteCluster, n => n != null && n.Parent == Root);

            myDragDropBehavior = new DragDropBehavior(Root);
            DropCommand = new DelegateCommand<NodeDropRequest>(myDragDropBehavior.ApplyDrop);
        }

        public ClusterTreeNode Root { get; private set; }

        public ICommand AddClusterCommand { get; private set; }

        private void OnAddCluster(ClusterTreeNode parent)
        {
            // avoid many intermediate updates
            myTransformationsObserver.ModuleChanged -= OnTransformationsChanged;

            var newClusterId = Guid.NewGuid().ToString();
            var captionModule = myPresentation.GetModule<ICaptionModule>();
            captionModule.Add(new Caption(newClusterId, "<new>"));

            new ChangeClusterAssignment(myPresentation)
                .Execute(t => t.AddCluster(newClusterId));

            // start new clusters folded
            new ChangeClusterFolding(myPresentation)
                .Execute(t => t.Toggle(newClusterId));

            // TODO: optimize!! (only update root)
            BuildTree();

            Root.Children.Single(n => n.Id == newClusterId).IsSelected = true;

            myTransformationsObserver.ModuleChanged += OnTransformationsChanged;
        }

        public ICommand DeleteClusterCommand { get; private set; }

        private void OnDeleteCluster(ClusterTreeNode node)
        {
            // avoid many intermediate updates
            myTransformationsObserver.ModuleChanged -= OnTransformationsChanged;

            var operation = new ChangeClusterAssignment(myPresentation);
            operation.Execute(t => t.HideCluster(node.Id));

            // TODO: optimize!! (only update root)
            BuildTree();

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

            var operation = new ChangeClusterAssignment(myPresentation);
            operation.Execute(t => t.AddToCluster(nodes, SelectedCluster));

            BuildTree();

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
                            Caption = TreeShowId ? n.Id : captionModule.Get(n.Id).DisplayText,
                            IsDropAllowed = false
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

                myNodeClusterCache = null;
            }
        }

        private void OnParentChanged(object sender, PropertyChangedEventArgs e)
        {
            var node = (ClusterTreeNode)sender;

            new ChangeClusterAssignment(myPresentation)
                .Execute(t => t.AddToCluster(node.Id, ((ClusterTreeNode)node.Parent).Id));
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
                    BuildTree();
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

                    OnPropertyChanged("PreviewNodes");
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
            if (myNodeClusterCache == null)
            {
                Debug.WriteLine("Rebuilding NodeClusterCache");

                myNodeClusterCache = new Dictionary<string, string>();

                foreach (ClusterTreeNode cluster in Root.Children)
                {
                    foreach (ClusterTreeNode clusterNode in cluster.Children)
                    {
                        myNodeClusterCache[clusterNode.Id] = cluster.Id;
                    }
                }
            }

            if (myNodeClusterCache.ContainsKey(node.Node.Id))
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
    }
}
