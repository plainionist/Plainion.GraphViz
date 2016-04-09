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

        [ImportingConstructor]
        public GraphViewerModel( IEventAggregator eventAggregator )
        {
            HideNodeCommand = new DelegateCommand<Node>( n => new HideSingleNode( Presentation ).Execute( n ) );
            ShowNodeWithSiblingsCommand = new DelegateCommand<Node>( n => new ShowNodeWithSiblings( Presentation ).Execute( n ) );
            ShowNodeWithIncomingCommand = new DelegateCommand<Node>( n => new ShowNodeWithIncomings( Presentation ).Execute( n ) );
            ShowNodeWithOutgoingCommand = new DelegateCommand<Node>( n => new ShowNodeWithOutgoings( Presentation ).Execute( n ) );

            CaptionToClipboardCommand = new DelegateCommand<Node>( n => Clipboard.SetText( Presentation.GetModule<CaptionModule>().Get( n.Id ).DisplayText ) );
            IdToClipboardCommand = new DelegateCommand<Node>( n => Clipboard.SetText( n.Id ) );

            GoToEdgeSourceCommand = new DelegateCommand<Edge>( edge => Navigation.NavigateTo( edge.Source ) );
            GoToEdgeTargetCommand = new DelegateCommand<Edge>( edge => Navigation.NavigateTo( edge.Target ) );

            ToggleClusterFoldingCommand = new DelegateCommand<Cluster>( c => new ChangeClusterFolding( Presentation ).Execute( t => t.Toggle( c.Id ) ) );
            UnfoldAndHidePrivateNodesCommand = new DelegateCommand<Cluster>( c => new UnfoldAndHidePrivateNodes( Presentation ).Execute( c ) );

            ShowCyclesCommand = new DelegateCommand( () => new ShowCycles( Presentation ).Execute(), () => Presentation != null );
            HideNodesWithoutEdgesCommand = new DelegateCommand( () => new HideNodesWithoutEdges( Presentation ).Execute(), () => Presentation != null );
            ShowNodesOutsideClustersCommand = new DelegateCommand( () => new ShowNodesOutsideClusters( Presentation ).Execute(), () => Presentation != null );
            FoldUnfoldAllClustersCommand = new DelegateCommand( () => new ChangeClusterFolding( Presentation ).FoldUnfoldAllClusters(), () => Presentation != null );
            InvalidateLayoutCommand = new DelegateCommand( () => Presentation.InvalidateLayout(), () => Presentation != null );

            AddToClusterCommand = new DelegateCommand<string>( c => new ChangeClusterAssignment( Presentation ).Execute( t => t.AddToCluster( GraphItemForContextMenu.Id, c ) ) );
            RemoveFromClusterCommand = new DelegateCommand<Node>( n => new ChangeClusterAssignment( Presentation ).Execute( t => t.RemoveFromClusters( GraphItemForContextMenu.Id ) ) );

            PrintGraphRequest = new InteractionRequest<IConfirmation>(); ;
            PrintGraphCommand = new DelegateCommand( OnPrintGrpah, () => Presentation != null );

            eventAggregator.GetEvent<NodeFocusedEvent>().Subscribe( OnEventFocused );

            Clusters = new ObservableCollection<ClusterWithCaption>();
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

        public DelegateCommand HideNodesWithoutEdgesCommand { get; private set; }

        public DelegateCommand ShowNodesOutsideClustersCommand { get; private set; }

        public DelegateCommand InvalidateLayoutCommand { get; private set; }

        public ICommand HideNodeCommand { get; private set; }

        public ICommand CaptionToClipboardCommand { get; private set; }

        public ICommand IdToClipboardCommand { get; private set; }

        public ICommand ShowNodeWithSiblingsCommand { get; private set; }

        public ICommand ShowNodeWithIncomingCommand { get; private set; }

        public ICommand ShowNodeWithOutgoingCommand { get; private set; }

        public ICommand GoToEdgeSourceCommand { get; private set; }

        public ICommand GoToEdgeTargetCommand { get; private set; }

        public ICommand ToggleClusterFoldingCommand { get; private set; }

        public ICommand UnfoldAndHidePrivateNodesCommand { get; private set; }

        public ICommand FoldUnfoldAllClustersCommand { get; private set; }

        protected override void OnModelPropertyChanged( string propertyName )
        {
            OnPropertyChanged( propertyName );

            if( propertyName == "Presentation" )
            {
                ShowCyclesCommand.RaiseCanExecuteChanged();
                HideNodesWithoutEdgesCommand.RaiseCanExecuteChanged();
                ShowNodesOutsideClustersCommand.RaiseCanExecuteChanged();
                InvalidateLayoutCommand.RaiseCanExecuteChanged();
                PrintGraphCommand.RaiseCanExecuteChanged();

                BuildClustersMenu();

                if( myTransformationsModuleObserver != null )
                {
                    myTransformationsModuleObserver.ModuleChanged -= OnTransformationsModuleChanged;
                    myTransformationsModuleObserver.Dispose();
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

        public IGraphItem GraphItemForContextMenu { get; set; }

        public ObservableCollection<ClusterWithCaption> Clusters { get; private set; }

        public ICommand AddToClusterCommand { get; private set; }

        public ICommand RemoveFromClusterCommand { get; private set; }
    }
}
