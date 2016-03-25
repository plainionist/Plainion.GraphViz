using System.ComponentModel.Composition;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using Plainion.GraphViz.Algorithms;
using Plainion.GraphViz.Infrastructure;
using Plainion.GraphViz.Infrastructure.ViewModel;
using Plainion.GraphViz.Model;
using Plainion.GraphViz.Presentation;
using Microsoft.Practices.Prism.Commands;
using Microsoft.Practices.Prism.Interactivity.InteractionRequest;
using Microsoft.Practices.Prism.PubSubEvents;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System;

namespace Plainion.GraphViz.Viewer.ViewModels
{
    [Export( typeof( GraphViewerModel ) )]
    class GraphViewerModel : ViewModelBase
    {
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

            ToggleClusterFoldingCommand = new DelegateCommand<Cluster>( OnToggleClusterFolding );

            ShowCyclesCommand = new DelegateCommand( () => new ShowCycles( Presentation ).Execute(), () => Presentation != null );
            HideNodesWithoutEdgesCommand = new DelegateCommand( () => new HideNodesWithoutEdges( Presentation ).Execute(), () => Presentation != null );
            ShowNodesOutsideClustersCommand = new DelegateCommand( () => new ShowNodesOutsideClusters( Presentation ).Execute(), () => Presentation != null );
            FoldUnfoldAllClustersCommand = new DelegateCommand( OnFoldUnfoldAllClusters, () => Presentation != null );
            InvalidateLayoutCommand = new DelegateCommand( () => Presentation.InvalidateLayout(), () => Presentation != null );

            PrintGraphRequest = new InteractionRequest<IConfirmation>(); ;
            PrintGraphCommand = new DelegateCommand( OnPrintGrpah, () => Presentation != null );

            eventAggregator.GetEvent<NodeFocusedEvent>().Subscribe( OnEventFocused );

            Clusters = new ObservableCollection<ClusterWithCaption>();

            AddToClusterCommand = new DelegateCommand<string>( OnAddToCluster );
            RemoveFromClusterCommand = new DelegateCommand<Node>( OnRemoveFromCluster );
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

        private void OnToggleClusterFolding( Cluster cluster )
        {
            var transformationModule = Presentation.GetModule<ITransformationModule>();

            var transformation = transformationModule.Items
                .OfType<ClusterFoldingTransformation>()
                .SingleOrDefault( t => t.Cluster.Id == cluster.Id );

            if( transformation == null )
            {
                transformationModule.Add( new ClusterFoldingTransformation( cluster, Presentation ) );
            }
            else
            {
                transformationModule.Remove( transformation );
            }
        }

        public ICommand FoldUnfoldAllClustersCommand { get; private set; }

        private void OnFoldUnfoldAllClusters()
        {
            var transformationModule = Presentation.GetModule<ITransformationModule>();

            var transformations = transformationModule.Items
                .OfType<ClusterFoldingTransformation>()
                .ToList();

            if( transformations.Count == 0 )
            {
                foreach( var cluster in Presentation.Graph.Clusters )
                {
                    transformationModule.Add( new ClusterFoldingTransformation( cluster, Presentation ) );
                }
            }
            else
            {
                foreach( var t in transformations )
                {
                    transformationModule.Remove( t );
                }
            }
        }

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

                Clusters.Clear();
                if( Presentation != null )
                {
                    var transformations = Presentation.GetModule<ITransformationModule>();
                    var captions = Presentation.GetModule<ICaptionModule>();
                    foreach( var cluster in transformations.Graph.Clusters )
                    {
                        Clusters.Add( new ClusterWithCaption
                        {
                            Id = cluster.Id,
                            Caption = captions.Get( cluster.Id ).DisplayText
                        } );
                    }
                }
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

        public IGraphItem GraphItemForContextMenu { get; set; }

        public ObservableCollection<ClusterWithCaption> Clusters { get; private set; }

        public ICommand AddToClusterCommand { get; private set; }

        private void OnAddToCluster( string clusterId )
        {
            ChangeClusterAssignment( t => t.AddToCluster( GraphItemForContextMenu.Id, clusterId ) );
        }

        private void ChangeClusterAssignment( Action<DynamicClusterTransformation> action )
        {
            var transformations = Presentation.GetModule<ITransformationModule>();
            var transformation = transformations.Items
                .OfType<DynamicClusterTransformation>()
                .SingleOrDefault();

            if( transformation == null )
            {
                transformation = new DynamicClusterTransformation();

                action( transformation );

                transformations.Add( transformation );
            }
            else
            {
                action( transformation );
            }
        }

        public ICommand RemoveFromClusterCommand { get; private set; }

        private void OnRemoveFromCluster( Node node )
        {
            ChangeClusterAssignment( t => t.RemoveFromClusters( GraphItemForContextMenu.Id ) );
        }
    }
}
