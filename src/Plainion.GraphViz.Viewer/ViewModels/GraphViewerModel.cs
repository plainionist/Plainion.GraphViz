using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel.Composition;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Input;
using Prism.Commands;
using Prism.Interactivity.InteractionRequest;
using Plainion.GraphViz.Algorithms;
using Plainion.GraphViz.Infrastructure;
using Plainion.GraphViz.Infrastructure.ViewModel;
using Plainion.GraphViz.Model;
using Plainion.GraphViz.Presentation;
using Prism.Events;


namespace Plainion.GraphViz.Viewer.ViewModels
{
    [Export(typeof(GraphViewerModel))]
    class GraphViewerModel : ViewModelBase
    {
        private IModuleChangedObserver myTransformationsModuleObserver;
        private IModuleChangedObserver mySelectionObserver;
        private IGraphItem myGraphItemForContextMenu;
        private bool mySelectionMenuUpdatePending;

        [ImportingConstructor]
        public GraphViewerModel(IEventAggregator eventAggregator)
        {
            RemoveNodeCommand = new DelegateCommand<Node>(
                n => Presentation.AddMask(new AddRemoveNodes(Presentation).Compute(GetSelectedNodes(n))),
                n => Presentation != null);

            RemoveAllButCommand = new DelegateCommand<Node>(
                n => OnRemoveAllBut(GetSelectedNodes(n)),
                n => Presentation != null);

            AddSourcesCommand = new DelegateCommand<Node>(
                n => Presentation.AddMask(new AddRemoveNodes(Presentation) { Add = true, SiblingsType = SiblingsType.Sources }.Compute(n)));

            AddTargetsCommand = new DelegateCommand<Node>(
                n => Presentation.AddMask(new AddRemoveNodes(Presentation) { Add = true, SiblingsType = SiblingsType.Targets }.Compute(n)));

            AddSiblingsCommand = new DelegateCommand<Node>(
                n => Presentation.AddMask(new AddRemoveNodes(Presentation) { SiblingsType = SiblingsType.Any }.Compute(n)));

            AddReachablesCommand = new DelegateCommand<Node>(
                n => Presentation.AddMask(new AddRemoveTransitiveHull(Presentation) { Add = true }.Compute(GetSelectedNodes(n))));

            RemoveSourcesCommand = new DelegateCommand<Node>(
                n => Presentation.AddMask(new AddRemoveNodes(Presentation) { Add = false, SiblingsType = SiblingsType.Sources }.Compute(n)));

            RemoveTargetsCommand = new DelegateCommand<Node>(
                n => Presentation.AddMask(new AddRemoveNodes(Presentation) { Add = false, SiblingsType = SiblingsType.Targets }.Compute(n)));

            RemoveUnreachableNodesCommand = new DelegateCommand<Node>(
                n => OnRemoveUnreachableNdoes(GetSelectedNodes(n)));

            SelectNodeCommand = new DelegateCommand<Node>(
                n => Presentation.GetPropertySetFor<Selection>().Get(n.Id).IsSelected = true);

            SelectNodeWithSourcesCommand = new DelegateCommand<Node>(
                n => Presentation.Select(n, SiblingsType.Sources));

            SelectNodeWithTargetsCommand = new DelegateCommand<Node>(
                n => Presentation.Select(n, SiblingsType.Targets));

            SelectNodeWithSiblingsCommand = new DelegateCommand<Node>(
                n => Presentation.Select(n, SiblingsType.Any));

            SelectReachablesCommand = new DelegateCommand<Node>(
                n => SelectReachables(GetSelectedNodes(n)));

            CaptionToClipboardCommand = new DelegateCommand<Node>(
                n => Clipboard.SetText(GetCaptionFromNode(n)));

            IdToClipboardCommand = new DelegateCommand<Node>(
                n => Clipboard.SetText(n.Id));

            GoToEdgeSourceCommand = new DelegateCommand<Edge>(
                edge => Navigation.NavigateTo(edge.Source));

            GoToEdgeTargetCommand = new DelegateCommand<Edge>(
                edge => Navigation.NavigateTo(edge.Target));

            ToggleClusterFoldingCommand = new DelegateCommand<Cluster>(
                c => Presentation.ChangeClusterFolding(ToggleClusterFolding(c)));

            RemoveNodesNotConnectedWithOutsideCommand = new DelegateCommand<Cluster>(
                c => Presentation.AddMask(new RemoveNodesNotConnectedWithOutsideCluster(Presentation).Compute(c)));

            RemoveNodesNotReachableFromOutsideCommand = new DelegateCommand<Cluster>(
                c => Presentation.AddMask(new RemoveNodesNotReachableFromOutsideCluster(Presentation).Compute(c)));

            RemoveNodesNotReachingOutsideCommand = new DelegateCommand<Cluster>(
                c => Presentation.AddMask(new RemoveNodesNotReachingOutsideCluster(Presentation).Compute(c)));

            CopyAllCaptionsToClipboardCommand = new DelegateCommand<Cluster>(
                c => Clipboard.SetDataObject(GetCaptionsOfAllVisibleNodesFrom(c)));

            CopyAllIdentifiersToClipboardCommand = new DelegateCommand<Cluster>(
                c => Clipboard.SetDataObject(GetIdentifiersOfAllVisibleNodesFrom(c)));

            ShowCyclesCommand = new DelegateCommand(
                () => Presentation.AddMask(new ShowCycles(Presentation).Compute()),
                () => Presentation != null);

            ShowNodesOutsideClustersCommand = new DelegateCommand(
                () => Presentation.AddMask(new RemoveClusters(Presentation).Compute()),
                () => Presentation != null);

            RemoveNodesWithoutEdgesCommand = new DelegateCommand(
                () => Presentation.AddMask(new RemoveNodesWithoutEdges(Presentation).Compute()),
                () => Presentation != null);

            FoldUnfoldAllClustersCommand = new DelegateCommand(
                () => Presentation.FoldUnfoldAllClusters(),
                () => Presentation != null);

            AddVisibleNodesOutsideClustersToClusterCommand = new DelegateCommand<string>(
                c => Presentation.AddToCluster(new GetNodesOutsideCluster(Presentation).Compute(c), c),
                c => Presentation != null);

            RemoveSelectionCommand = new DelegateCommand(
                () => Presentation.GetPropertySetFor<Selection>().Clear(),
                () => Presentation != null);

            InvertSelectionCommand = new DelegateCommand(
                () => OnInvertSelection(),
                () => Presentation != null);

            HomeCommand = new DelegateCommand(
                () => Navigation.HomeZoomPan(),
                () => Presentation != null);

            InvalidateLayoutCommand = new DelegateCommand(
                () => Presentation.InvalidateLayout(),
                () => Presentation != null);

            AddToClusterCommand = new DelegateCommand<string>(
                c => Presentation.ChangeClusterAssignment(t => t.AddToCluster(GraphItemForContextMenu.Id, c)));

            AddWithSelectedToClusterCommand = new DelegateCommand<string>(
                c => Presentation.ChangeClusterAssignment(t => t.AddToCluster(GetSelectedNodes(null)
                    .Select(n => n.Id).ToArray(), c)));

            RemoveFromClusterCommand = new DelegateCommand<Node>(
                node => Presentation.ChangeClusterAssignment(t => t.RemoveFromClusters(GetSelectedNodes(node)
                    .Select(n => n.Id).ToArray())));

            TraceToCommand = new DelegateCommand<Node>(
                n => Presentation.AddMask(new ShowPath(Presentation).Compute((Node)GraphItemForContextMenu, n)));

            PrintGraphRequest = new InteractionRequest<IConfirmation>();
            PrintGraphCommand = new DelegateCommand(OnPrintGrpah, () => Presentation != null);

            eventAggregator.GetEvent<NodeFocusedEvent>().Subscribe(OnEventFocused);

            Clusters = new ObservableCollection<ClusterWithCaption>();
            SelectedNodes = new ObservableCollection<NodeWithCaption>();
        }

        private void OnRemoveAllBut(Node[] nodes)
        {
            var mask = new AddRemoveNodes(Presentation).Compute(nodes);
            mask.Invert(Presentation);
            Presentation.AddMask(mask);
        }

        private void OnInvertSelection()
        {
            var selection = Presentation.GetPropertySetFor<Selection>();

            var graph = Presentation.TransformedGraph();
            var inversion = graph.Nodes
                .Where(Presentation.Picking.Pick)
                .Where(n => !(selection.TryGet(n.Id)?.IsSelected ?? false))
                .Select(n => n.Id)
                .ToList();

            var invertedSelection = new HashSet<string>(inversion);

            selection.Clear();

            foreach (var n in invertedSelection)
            {
                selection.Get(n).IsSelected = true;
            }

            foreach (var edge in graph.Edges.Where(Presentation.Picking.Pick))
            {
                if (invertedSelection.Contains(edge.Source.Id) && invertedSelection.Contains(edge.Target.Id))
                {
                    selection.Get(edge.Id).IsSelected = true;
                }
            }
        }

        private void OnRemoveUnreachableNdoes(Node[] nodes)
        {
            var mask = new AddRemoveTransitiveHull(Presentation) { Add = false }.Compute(nodes);
            mask.Invert(Presentation);

            Presentation.AddMask(mask);
        }

        private void SelectReachables(Node[] nodes)
        {
            // pass "Add = false" as we want the transitive hull of the visible nodes
            var mask = new AddRemoveTransitiveHull(Presentation) { Add = false }.Compute(nodes);

            var selection = Presentation.GetPropertySetFor<Selection>();
            var edges = Presentation.GetModule<ITransformationModule>().Graph.Edges
                .Where(Presentation.Picking.Pick)
                // check for "false" as we have a "hide" mask
                .Where(e => mask.IsSet(e.Source) == false && mask.IsSet(e.Target) == false);

            foreach (var e in edges)
            {
                selection.Get(e.Id).IsSelected = true;
                selection.Get(e.Source.Id).IsSelected = true;
                selection.Get(e.Target.Id).IsSelected = true;
            }
        }

        private string GetCaptionFromNode(Node n)
        {
            return Presentation.GetModule<ICaptionModule>().Get(n.Id).DisplayText;
        }

        private string GetCaptionsOfAllVisibleNodesFrom(Cluster c)
        {
            var visibleNodes = c.Nodes
                .Where(n => Presentation.Picking.Pick(n));

            var str = new StringBuilder();
            foreach (var n in visibleNodes)
            {
                str.AppendLine(GetCaptionFromNode(n));
            }
            return str.ToString();
        }

        private string GetIdentifiersOfAllVisibleNodesFrom(Cluster c)
        {
            var visibleNodes = c.Nodes
                .Where(n => Presentation.Picking.Pick(n));

            var str = new StringBuilder();
            foreach (var n in visibleNodes)
            {
                str.AppendLine(n.Id);
            }
            return str.ToString();
        }

        private Action<ClusterFoldingTransformation> ToggleClusterFolding(Cluster commandParamter)
        {
            return t =>
            {
                foreach (var c in GetSelectedClusters(commandParamter))
                {
                    t.Toggle(c.Id);
                }
            };
        }

        // by convention: if given commandParameter is null then "this and all selected" nodes are relevant
        private Node[] GetSelectedNodes(Node commandParamter)
        {
            if (commandParamter != null)
            {
                return new[] { commandParamter };
            }

            var selectionModule = Presentation.GetPropertySetFor<Selection>();
            try
            {
                // avoid creating a lot of selection objects just to clear the module 
                // in the next line
                var nodes = Presentation.GetModule<ITransformationModule>().Graph.Nodes;
                return selectionModule.Items
                    .Where(x => x.IsSelected)
                    .Select(x => nodes.FirstOrDefault(n => n.Id == x.OwnerId))
                    .Concat(new[] { (Node)GraphItemForContextMenu })
                    .Where(n => n != null)
                    .ToArray();
            }
            finally
            {
                // the selection is considered to be temporary for this operation only
                // -> remove selection again because relevant nodes have been picked up
                selectionModule.Clear();
            }
        }

        // by convention: if given commandParameter is null then "this and all selected" cluster are relevant
        private Cluster[] GetSelectedClusters(Cluster commandParamter)
        {
            if (commandParamter != null)
            {
                return new[] { commandParamter };
            }

            var selectionModule = Presentation.GetPropertySetFor<Selection>();
            try
            {
                // avoid creating a lot of selection objects just to clear the module 
                // in the next line
                var clusters = Presentation.GetModule<ITransformationModule>().Graph.Clusters;
                return selectionModule.Items
                    .Where(x => x.IsSelected)
                    .Select(x => clusters.FirstOrDefault(c => c.Id == x.OwnerId))
                    .Concat(new[] { (Cluster)GraphItemForContextMenu })
                    .Where(n => n != null)
                    .ToArray();
            }
            finally
            {
                // the selection is considered to be temporary for this operation only
                // -> remove selection again because relevant nodes have been picked up
                selectionModule.Clear();
            }
        }

        private bool CanUnfold(Cluster cluster)
        {
            if (GraphItemForContextMenu == null)
            {
                // context menu was open and then right click somewhere else
                return false;
            }

            var transformation = Presentation.GetModule<ITransformationModule>().Items
                .OfType<ClusterFoldingTransformation>()
                .SingleOrDefault();

            return transformation != null && transformation.Clusters.Contains(GraphItemForContextMenu.Id);
        }

        private void OnEventFocused(Node node)
        {
            if (node != null)
            {
                Navigation.NavigateTo(node);

                //myGraphViewer.GraphVisual.Presentation.GetModuleFor<SelectionState>().Get( selectedNode.Id ).IsSelected = true;
            }
        }

        public InteractionRequest<IConfirmation> PrintGraphRequest { get; private set; }

        public DelegateCommand PrintGraphCommand { get; private set; }

        private void OnPrintGrpah()
        {
            var notification = new Confirmation();
            notification.Title = "Plainion.GraphViz.Viewer";

            PrintGraphRequest.Raise(notification, c => { });
        }

        public IGraphViewNavigation Navigation { get; set; }

        public DelegateCommand ShowCyclesCommand { get; private set; }

        public DelegateCommand RemoveNodesWithoutEdgesCommand { get; private set; }

        public DelegateCommand ShowNodesOutsideClustersCommand { get; private set; }

        public DelegateCommand RemoveSelectionCommand { get; private set; }

        public DelegateCommand InvertSelectionCommand { get; private set; }

        public DelegateCommand HomeCommand { get; private set; }

        public DelegateCommand InvalidateLayoutCommand { get; private set; }

        public DelegateCommand<Node> RemoveNodeCommand { get; private set; }

        public DelegateCommand<Node> RemoveAllButCommand { get; private set; }

        public ICommand RemoveSourcesCommand { get; private set; }

        public ICommand RemoveTargetsCommand { get; private set; }

        public ICommand RemoveUnreachableNodesCommand { get; private set; }

        public ICommand SelectNodeCommand { get; private set; }

        public ICommand SelectNodeWithSourcesCommand { get; private set; }

        public ICommand SelectNodeWithTargetsCommand { get; private set; }

        public ICommand SelectNodeWithSiblingsCommand { get; private set; }

        public ICommand SelectReachablesCommand { get; private set; }

        public ICommand CaptionToClipboardCommand { get; private set; }

        public ICommand CopyAllCaptionsToClipboardCommand { get; private set; }

        public ICommand CopyAllIdentifiersToClipboardCommand { get; private set; }

        public ICommand IdToClipboardCommand { get; private set; }

        public ICommand AddSiblingsCommand { get; private set; }

        public ICommand AddReachablesCommand { get; private set; }

        public ICommand AddSourcesCommand { get; private set; }

        public ICommand AddTargetsCommand { get; private set; }

        public ICommand GoToEdgeSourceCommand { get; private set; }

        public ICommand GoToEdgeTargetCommand { get; private set; }

        public ICommand ToggleClusterFoldingCommand { get; private set; }

        public DelegateCommand<Cluster> RemoveNodesNotConnectedWithOutsideCommand { get; private set; }

        public DelegateCommand<Cluster> RemoveNodesNotReachableFromOutsideCommand { get; private set; }

        public DelegateCommand<Cluster> RemoveNodesNotReachingOutsideCommand { get; private set; }

        public DelegateCommand FoldUnfoldAllClustersCommand { get; private set; }

        public DelegateCommand<string> AddVisibleNodesOutsideClustersToClusterCommand { get; private set; }

        protected override void OnModelPropertyChanged(string propertyName)
        {
            RaisePropertyChanged(propertyName);

            if (propertyName == "Presentation")
            {
                RemoveNodeCommand.RaiseCanExecuteChanged();
                RemoveAllButCommand.RaiseCanExecuteChanged();
                ShowCyclesCommand.RaiseCanExecuteChanged();
                RemoveNodesWithoutEdgesCommand.RaiseCanExecuteChanged();
                ShowNodesOutsideClustersCommand.RaiseCanExecuteChanged();
                FoldUnfoldAllClustersCommand.RaiseCanExecuteChanged();
                AddVisibleNodesOutsideClustersToClusterCommand.RaiseCanExecuteChanged();
                RemoveSelectionCommand.RaiseCanExecuteChanged();
                InvertSelectionCommand.RaiseCanExecuteChanged();
                HomeCommand.RaiseCanExecuteChanged();
                InvalidateLayoutCommand.RaiseCanExecuteChanged();
                PrintGraphCommand.RaiseCanExecuteChanged();

                BuildClustersMenu();
                BuildSelectedNodesMenu();

                if (myTransformationsModuleObserver != null)
                {
                    mySelectionObserver.ModuleChanged -= OnSelectionChanged;
                    mySelectionObserver.Dispose();
                    mySelectionObserver = null;

                    myTransformationsModuleObserver.ModuleChanged -= OnTransformationsModuleChanged;
                    myTransformationsModuleObserver.Dispose();
                    myTransformationsModuleObserver = null;
                }

                if (Presentation != null)
                {
                    var transformations = Presentation.GetModule<ITransformationModule>();
                    myTransformationsModuleObserver = transformations.CreateObserver();
                    myTransformationsModuleObserver.ModuleChanged += OnTransformationsModuleChanged;

                    mySelectionObserver = Presentation.GetPropertySetFor<Selection>().CreateObserver();
                    mySelectionObserver.ModuleChanged += OnSelectionChanged;
                }
            }
        }

        private void BuildClustersMenu()
        {
            Clusters.Clear();

            if (Presentation == null)
            {
                return;
            }

            var transformations = Presentation.GetModule<ITransformationModule>();
            var captions = Presentation.GetModule<ICaptionModule>();

            var clusters = transformations.Graph.Clusters
                .Union(Presentation.Graph.Clusters)
                .Select(c => c.Id)
                .Distinct()
                .Select(id => new ClusterWithCaption
                {
                    Id = id,
                    Caption = captions.Get(id).DisplayText
                })
                .OrderBy(id => id.Caption);

            foreach (var cluster in clusters)
            {
                Clusters.Add(cluster);
            }
        }

        private void BuildSelectedNodesMenu()
        {
            SelectedNodes.Clear();

            if (Presentation == null)
            {
                return;
            }

            var transformations = Presentation.GetModule<ITransformationModule>();
            var captions = Presentation.GetModule<ICaptionModule>();
            var selections = Presentation.GetPropertySetFor<Selection>();

            var nodes = transformations.Graph.Nodes
                .Union(Presentation.Graph.Nodes)
                .Where(n => selections.TryGet(n.Id)?.IsSelected ?? false)
                .Distinct()
                .Select(n => new NodeWithCaption(n, captions.Get(n.Id).DisplayText))
                .OrderBy(n => n.DisplayText);

            foreach (var n in nodes)
            {
                SelectedNodes.Add(n);
            }

            mySelectionMenuUpdatePending = false;
        }

        private void OnTransformationsModuleChanged(object sender, EventArgs e)
        {
            BuildClustersMenu();
        }

        private void OnSelectionChanged(object sender, EventArgs e)
        {
            if (!mySelectionMenuUpdatePending)
            {
                mySelectionMenuUpdatePending = true;

                // there could come many notifications in a row due to the "on demand create" Get() behavior
                // -> lets queue into message pump so that we skip many or all notifications and just collect
                //    later the final state
                Application.Current.Dispatcher.BeginInvoke(new Action(BuildSelectedNodesMenu));
            }
        }

        public IGraphPresentation Presentation
        {
            get { return Model.Presentation; }
        }

        public ILayoutEngine LayoutEngine
        {
            get { return Model.LayoutEngine; }
        }

        public IGraphItem GraphItemForContextMenu
        {
            get { return myGraphItemForContextMenu; }
            set
            {
                if (SetProperty(ref myGraphItemForContextMenu, value))
                {
                    // otherwise we get exception when we open context menu on cluster and without choosing any menu
                    // just open context menu on e.g. node
                    if (myGraphItemForContextMenu is Cluster)
                    {
                        RemoveNodesNotConnectedWithOutsideCommand.RaiseCanExecuteChanged();
                        RemoveNodesNotReachableFromOutsideCommand.RaiseCanExecuteChanged();
                        RemoveNodesNotReachingOutsideCommand.RaiseCanExecuteChanged();
                    }
                }
            }
        }

        public ObservableCollection<ClusterWithCaption> Clusters { get; private set; }

        public ObservableCollection<NodeWithCaption> SelectedNodes { get; private set; }

        public ICommand AddToClusterCommand { get; private set; }

        public ICommand AddWithSelectedToClusterCommand { get; private set; }

        public ICommand RemoveFromClusterCommand { get; private set; }

        public ICommand TraceToCommand { get; private set; }
    }
}
