using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Windows.Input;
using Plainion.Collections;
using Plainion.GraphViz.Presentation;
using Plainion.GraphViz.Viewer.Abstractions.ViewModel;
using Prism.Commands;
using Prism.Mvvm;

namespace Plainion.GraphViz.Modules.Analysis.Clusters
{
    class ClusterEditorViewModel : ViewModelBase
    {
        private IGraphPresentation myPresentation;
        private IModuleChangedObserver myTransformationsObserver;

        public ClusterEditorViewModel(IDomainModel model)
            : base(model)
        {
            Root = new NodeViewModel(null, null, NodeType.Root);

            AddNodesToClusterCommand = new DelegateCommand(OnAddNodesToCluster, () => Tree.SelectedCluster != null);

            DropCommand = new DelegateCommand<NodeDropRequest>(OnDrop);

            Preview = new PreviewViewModel(model, this);
            Tree = new TreeEditorViewModel(model, this);
        }

        public NodeViewModel Root { get; }
        public PreviewViewModel Preview { get; }
        public TreeEditorViewModel Tree { get; }

        public void CreateNewCluster(NodeViewModel parent)
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

        public void DeleteNode(NodeViewModel node)
        {
            if (node.Type == NodeType.Cluster)
            {
                // avoid many intermediate updates
                myTransformationsObserver.ModuleChanged -= OnTransformationsChanged;

                myPresentation.DynamicClusters().HideCluster(node.Id);

                // the tree might have been rebuilt - we have to search by id
                Root.Children.Remove(Root.Children.Single(x => x.Id == node.Id));

                if (node.Id == Tree.SelectedCluster)
                {
                    Tree.SelectedCluster = null;
                }

                Preview.OnClusterDeleted(node);

                myTransformationsObserver.ModuleChanged += OnTransformationsChanged;
            }
            else
            {
                myPresentation.DynamicClusters().RemoveFromClusters(node.Id);

                Preview.OnNodeRemovedFromCluster(node);
            }
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
                DeleteNode(request.DroppedNode);
            }
        }

        public DelegateCommand AddNodesToClusterCommand { get; }

        private void OnAddNodesToCluster()
        {
            // avoid many intermediate updates
            myTransformationsObserver.ModuleChanged -= OnTransformationsChanged;

            var nodes = Preview.PreviewNodes
                .Cast<NodeWithCaption>()
                .Select(n => n.Node.Id)
                .ToList();

            myPresentation.DynamicClusters().AddToCluster(nodes, Tree.SelectedCluster);

            // update tree
            {
                var clusterNode = Root.Children.Single(n => n.Id == Tree.SelectedCluster);

                var captionModule = myPresentation.GetModule<ICaptionModule>();

                var newTreeNodes = nodes
                    .Select(n => new NodeViewModel(myPresentation, n, NodeType.Node)
                    {
                        Parent = clusterNode,
                        Caption = captionModule.Get(n).DisplayText,
                        ShowId = Tree.ShowNodeId
                    });
                clusterNode.Children.AddRange(newTreeNodes);

                // register for notifications after tree is built to avoid intermediate states getting notified

                foreach (var node in newTreeNodes)
                {
                    PropertyChangedEventManager.AddHandler(node, OnSelectionChanged, PropertySupport.ExtractPropertyName(() => node.IsSelected));
                    PropertyChangedEventManager.AddHandler(node, OnParentChanged, PropertySupport.ExtractPropertyName(() => node.Parent));

                    Preview.OnNodeAddedToCluster(node, clusterNode);
                }
            }

            myTransformationsObserver.ModuleChanged += OnTransformationsChanged;

            Preview.Filter = null;
            Preview.PreviewNodes.Refresh();
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

            // register for updates only AFTER tree is built up completely to avoid getting notified by the built up process
            var transformationModule = Model.Presentation.GetModule<ITransformationModule>();
            myTransformationsObserver = transformationModule.CreateObserver();
            myTransformationsObserver.ModuleChanged += OnTransformationsChanged;
        }

        private void BuildTree()
        {
            using var _ = new Profile("BuildTree");

            var expandedClusterIds = Root.Children
                .Where(x => x.IsExpanded)
                .Select(x => x.Id)
                .ToHashSet();

            Root.Children.Clear();

            Tree.SelectedCluster = null;

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
                        ShowId = Tree.ShowNodeId
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

            Root.ApplyFilter(Tree.Filter);
        }

        private void OnParentChanged(object sender, PropertyChangedEventArgs e)
        {
            var node = (NodeViewModel)sender;
            myPresentation.DynamicClusters().AddToCluster(node.Id, node.Parent.Id);
        }

        private void OnSelectionChanged(object sender, PropertyChangedEventArgs e)
        {
            var node = (NodeViewModel)sender;
            Tree.SelectCluster(node);
        }

        private void OnTransformationsChanged(object sender, EventArgs e)
        {
            BuildTree();
            Preview.OnTransformationsChanged();
        }

        public void OnCusterSelected(string clusterId)
        {
            var captionModule = myPresentation.GetModule<ICaptionModule>();
            Preview.AddButtonCaption = clusterId != null ? "Add to '" + captionModule.Get(clusterId).DisplayText + "'" : "Add ...";
            AddNodesToClusterCommand.RaiseCanExecuteChanged();
        }
    }
}
