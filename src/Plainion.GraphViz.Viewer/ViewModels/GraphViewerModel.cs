using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel.Composition;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using Microsoft.Practices.Prism.Commands;
using Microsoft.Practices.Prism.Interactivity.InteractionRequest;
using Microsoft.Practices.Prism.PubSubEvents;
using Plainion.GraphViz.Algorithms;
using Plainion.GraphViz.Infrastructure;
using Plainion.GraphViz.Infrastructure.ViewModel;
using Plainion.GraphViz.Model;
using Plainion.GraphViz.Presentation;

namespace Plainion.GraphViz.Viewer.ViewModels
{
    [Export( typeof( GraphViewerModel ) )]
    class GraphViewerModel : ViewModelBase
    {
        private IModuleChangedObserver myTransformationsModuleObserver;
        private IGraphItem myGraphItemForContextMenu;

        [ImportingConstructor]
        public GraphViewerModel( IEventAggregator eventAggregator )
        {
            HideNodeCommand = new DelegateCommand<Node>( n => new ShowHideNodes( Presentation, false ).Execute( GetRelevantNodes( n ) ) );
            ShowNodeCommand = new DelegateCommand<Node>( n => new ShowHideNodes( Presentation, true ).Execute( GetRelevantNodes( n ) ) );

            ShowNodeWithSiblingsCommand = new DelegateCommand<Node>( n => new ShowSiblings( Presentation ).Execute( n ) );
            ShowNodeWithIncomingCommand = new DelegateCommand<Node>( n => new ShowHideIncomings( Presentation ).Execute( n, show: true ) );
            ShowNodeWithOutgoingCommand = new DelegateCommand<Node>( n => new ShowHideOutgoings( Presentation ).Execute( n, show: true ) );

            HideIncomingCommand = new DelegateCommand<Node>( n => new ShowHideIncomings( Presentation ).Execute( n, show: false ) );
            HideOutgoingCommand = new DelegateCommand<Node>( n => new ShowHideOutgoings( Presentation ).Execute( n, show: false ) );
            RemoveUnreachableNodesCommand = new DelegateCommand<Node>( n => new RemoveUnreachableNodes( Presentation ).Execute( n ) );

            CaptionToClipboardCommand = new DelegateCommand<Node>( n => Clipboard.SetText( Presentation.GetModule<CaptionModule>().Get( n.Id ).DisplayText ) );
            IdToClipboardCommand = new DelegateCommand<Node>( n => Clipboard.SetText( n.Id ) );

            GoToEdgeSourceCommand = new DelegateCommand<Edge>( edge => Navigation.NavigateTo( edge.Source ) );
            GoToEdgeTargetCommand = new DelegateCommand<Edge>( edge => Navigation.NavigateTo( edge.Target ) );

            ToggleClusterFoldingCommand = new DelegateCommand<Cluster>( c => new ChangeClusterFolding( Presentation ).Execute( t => t.Toggle( c.Id ) ) );
            UnfoldAndHidePrivateNodesCommand = new DelegateCommand<Cluster>( c => new UnfoldAndHide( Presentation ).Execute( c, NodeType.AllSiblings ), CanUnfold );
            UnfoldAndHideAllButTargetsCommand = new DelegateCommand<Cluster>( c => new UnfoldAndHide( Presentation ).Execute( c, NodeType.Targets ), CanUnfold );
            UnfoldAndHideAllButSourcesCommand = new DelegateCommand<Cluster>( c => new UnfoldAndHide( Presentation ).Execute( c, NodeType.Sources ), CanUnfold );

            ShowCyclesCommand = new DelegateCommand( () => new ShowCycles( Presentation ).Execute(), () => Presentation != null );
            ShowNodesOutsideClustersCommand = new DelegateCommand( () => new ShowNodesOutsideClusters( Presentation ).Execute(), () => Presentation != null );

            RemoveNodesWithoutEdgesCommand = new DelegateCommand( () => new RemoveNodesWithoutEdges( Presentation ).Execute(), () => Presentation != null );
            RemoveNodesReachableFromMultipleClustersCommand = new DelegateCommand( () => new RemoveNodesReachableFromMultipleClusters( Presentation ).Execute(), () => Presentation != null );

            FoldUnfoldAllClustersCommand = new DelegateCommand( () => new ChangeClusterFolding( Presentation ).FoldUnfoldAllClusters(), () => Presentation != null );
            AddVisibleNodesOutsideClustersToClusterCommand = new DelegateCommand<string>( c => new AddVisibleNodesOutsideClustersToCluster( Presentation ).Execute( c ), c => Presentation != null );
            InvalidateLayoutCommand = new DelegateCommand( () => Presentation.InvalidateLayout(), () => Presentation != null );

            AddToClusterCommand = new DelegateCommand<string>( c => new ChangeClusterAssignment( Presentation ).Execute( t => t.AddToCluster( GraphItemForContextMenu.Id, c ) ) );
            AddWithSelectedToClusterCommand = new DelegateCommand<string>( c => new ChangeClusterAssignment( Presentation )
                .Execute( t => t.AddToCluster( GetRelevantNodes( null )
                    .Select( n => n.Id ).ToArray(), c ) ) );
            RemoveFromClusterCommand = new DelegateCommand<Node>( node => new ChangeClusterAssignment( Presentation )
                .Execute( t => t.RemoveFromClusters( GetRelevantNodes( node )
                    .Select( n => n.Id ).ToArray() ) ) );

            PrintGraphRequest = new InteractionRequest<IConfirmation>();
            PrintGraphCommand = new DelegateCommand( OnPrintGrpah, () => Presentation != null );

            eventAggregator.GetEvent<NodeFocusedEvent>().Subscribe( OnEventFocused );

            Clusters = new ObservableCollection<ClusterWithCaption>();
        }

        // by convention: if given commandParameter is null then "this and all selected" nodes are relevant
        private Node[] GetRelevantNodes( Node commandParamter )
        {
            if( commandParamter != null )
            {
                return new[] { commandParamter };
            }

            var selectionModule = Presentation.GetPropertySetFor<Selection>();
            try
            {
                return Presentation.GetModule<ITransformationModule>().Graph.Nodes
                    .Where( n => selectionModule.Get( n.Id ).IsSelected )
                    .Concat( new[] { ( Node )GraphItemForContextMenu } )
                    .ToArray();
            }
            finally
            {
                // the selection is considered to be temporary for this operation only
                // -> remove selection again because relevant nodes have been picked up
                selectionModule.Clear();
            }
        }

        private bool CanUnfold( Cluster cluster )
        {
            var transformation = Presentation.GetModule<ITransformationModule>().Items
                .OfType<ClusterFoldingTransformation>()
                .SingleOrDefault();

            return transformation != null && transformation.Clusters.Contains( GraphItemForContextMenu.Id );
        }

        private void OnEventFocused( Node node )
        {
            if( node != null )
            {
                Navigation.NavigateTo( node );

                //myGraphViewer.GraphVisual.Presentation.GetModuleFor<SelectionState>().Get( selectedNode.Id ).IsSelected = true;
            }
        }

        public InteractionRequest<IConfirmation> PrintGraphRequest { get; private set; }

        public DelegateCommand PrintGraphCommand { get; private set; }

        private void OnPrintGrpah()
        {
            var notification = new Confirmation();
            notification.Title = "Plainion.GraphViz.Viewer";

            PrintGraphRequest.Raise( notification, c => { } );
        }

        public IGraphViewNavigation Navigation { get; set; }

        public DelegateCommand ShowCyclesCommand { get; private set; }

        public DelegateCommand RemoveNodesWithoutEdgesCommand { get; private set; }

        public DelegateCommand ShowNodesOutsideClustersCommand { get; private set; }

        public DelegateCommand RemoveNodesReachableFromMultipleClustersCommand { get; private set; }

        public DelegateCommand InvalidateLayoutCommand { get; private set; }

        public ICommand HideNodeCommand { get; private set; }

        public ICommand HideIncomingCommand { get; private set; }

        public ICommand HideOutgoingCommand { get; private set; }

        public ICommand RemoveUnreachableNodesCommand { get; private set; }

        public ICommand CaptionToClipboardCommand { get; private set; }

        public ICommand IdToClipboardCommand { get; private set; }

        public ICommand ShowNodeCommand { get; private set; }

        public ICommand ShowNodeWithSiblingsCommand { get; private set; }

        public ICommand ShowNodeWithIncomingCommand { get; private set; }

        public ICommand ShowNodeWithOutgoingCommand { get; private set; }

        public ICommand GoToEdgeSourceCommand { get; private set; }

        public ICommand GoToEdgeTargetCommand { get; private set; }

        public ICommand ToggleClusterFoldingCommand { get; private set; }

        public DelegateCommand<Cluster> UnfoldAndHidePrivateNodesCommand { get; private set; }

        public DelegateCommand<Cluster> UnfoldAndHideAllButTargetsCommand { get; private set; }

        public DelegateCommand<Cluster> UnfoldAndHideAllButSourcesCommand { get; private set; }

        public DelegateCommand FoldUnfoldAllClustersCommand { get; private set; }

        public DelegateCommand<string> AddVisibleNodesOutsideClustersToClusterCommand { get; private set; }

        protected override void OnModelPropertyChanged( string propertyName )
        {
            OnPropertyChanged( propertyName );

            if( propertyName == "Presentation" )
            {
                ShowCyclesCommand.RaiseCanExecuteChanged();
                RemoveNodesWithoutEdgesCommand.RaiseCanExecuteChanged();
                ShowNodesOutsideClustersCommand.RaiseCanExecuteChanged();
                RemoveNodesReachableFromMultipleClustersCommand.RaiseCanExecuteChanged();
                FoldUnfoldAllClustersCommand.RaiseCanExecuteChanged();
                AddVisibleNodesOutsideClustersToClusterCommand.RaiseCanExecuteChanged();
                InvalidateLayoutCommand.RaiseCanExecuteChanged();
                PrintGraphCommand.RaiseCanExecuteChanged();

                BuildClustersMenu();

                if( myTransformationsModuleObserver != null )
                {
                    myTransformationsModuleObserver.ModuleChanged -= OnTransformationsModuleChanged;
                    myTransformationsModuleObserver.Dispose();
                    myTransformationsModuleObserver = null;
                }

                if( Presentation != null )
                {
                    var transformations = Presentation.GetModule<ITransformationModule>();
                    myTransformationsModuleObserver = transformations.CreateObserver();
                    myTransformationsModuleObserver.ModuleChanged += OnTransformationsModuleChanged;
                }
            }
        }

        private void BuildClustersMenu()
        {
            Clusters.Clear();

            if( Presentation == null )
            {
                return;
            }

            var transformations = Presentation.GetModule<ITransformationModule>();
            var captions = Presentation.GetModule<ICaptionModule>();

            var clusters = transformations.Graph.Clusters
                .Union( Presentation.Graph.Clusters )
                .Select( c => c.Id )
                .Distinct()
                .Select( id => new ClusterWithCaption
                {
                    Id = id,
                    Caption = captions.Get( id ).DisplayText
                } )
                .OrderBy( id => id.Caption );

            foreach( var cluster in clusters )
            {
                Clusters.Add( cluster );
            }
        }

        private void OnTransformationsModuleChanged( object sender, EventArgs e )
        {
            BuildClustersMenu();
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
                if( SetProperty( ref myGraphItemForContextMenu, value ) )
                {
                    UnfoldAndHidePrivateNodesCommand.RaiseCanExecuteChanged();
                    UnfoldAndHideAllButTargetsCommand.RaiseCanExecuteChanged();
                    UnfoldAndHideAllButSourcesCommand.RaiseCanExecuteChanged();
                }
            }
        }

        public ObservableCollection<ClusterWithCaption> Clusters { get; private set; }

        public ICommand AddToClusterCommand { get; private set; }

        public ICommand AddWithSelectedToClusterCommand { get; private set; }

        public ICommand RemoveFromClusterCommand { get; private set; }
    }
}
