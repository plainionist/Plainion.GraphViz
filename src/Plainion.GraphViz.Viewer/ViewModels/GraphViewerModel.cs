using System.ComponentModel.Composition;
using System.Windows.Input;
using Plainion.GraphViz.Algorithms;
using Plainion.GraphViz.Infrastructure;
using Plainion.GraphViz.Infrastructure.ViewModel;
using Plainion.GraphViz.Model;
using Plainion.GraphViz.Presentation;
using Microsoft.Practices.Prism.Commands;
using Microsoft.Practices.Prism.Interactivity.InteractionRequest;
using Microsoft.Practices.Prism.PubSubEvents;

namespace Plainion.GraphViz.Viewer.ViewModels
{
    [Export( typeof( GraphViewerModel ) )]
    public class GraphViewerModel : ViewModelBase
    {
        [ImportingConstructor]
        public GraphViewerModel( IEventAggregator eventAggregator )
        {
            HideNodeCommand = new DelegateCommand<Node>( n => new HideSingleNode( Presentation ).Execute( n ) );
            ShowNodeWithSiblingsCommand = new DelegateCommand<Node>( n => new ShowNodeWithSiblings( Presentation ).Execute( n ) );
            ShowNodeWithIncomingCommand = new DelegateCommand<Node>( n => new ShowNodeWithIncomings( Presentation ).Execute( n ) );
            ShowNodeWithOutgoingCommand = new DelegateCommand<Node>( n => new ShowNodeWithOutgoings( Presentation ).Execute( n ) );

            GoToEdgeSourceCommand = new DelegateCommand<Edge>( edge => Navigation.NavigateTo( edge.Source ) );
            GoToEdgeTargetCommand = new DelegateCommand<Edge>( edge => Navigation.NavigateTo( edge.Target ) );

            InvalidateLayoutCommand = new DelegateCommand( OnInvalidateLayout, () => Presentation != null );

            PrintGraphRequest = new InteractionRequest<IConfirmation>(); ;
            PrintGraphCommand = new DelegateCommand( OnPrintGrpah, () => Presentation != null );

            eventAggregator.GetEvent<NodeFocusedEvent>().Subscribe( OnEventFocused );
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

        public DelegateCommand PrintGraphCommand
        {
            get;
            private set;
        }

        private void OnPrintGrpah()
        {
            var notification = new Confirmation();
            notification.Title = "Plainion.GraphViz.Viewer";

            PrintGraphRequest.Raise( notification, c => { } );
        }

        public IGraphViewNavigation Navigation
        {
            get;
            set;
        }

        public DelegateCommand InvalidateLayoutCommand
        {
            get;
            private set;
        }

        private void OnInvalidateLayout()
        {
            Presentation.InvalidateLayout();
        }

        public ICommand HideNodeCommand
        {
            get;
            private set;
        }

        public ICommand ShowNodeWithSiblingsCommand
        {
            get;
            private set;
        }

        public ICommand ShowNodeWithIncomingCommand
        {
            get;
            private set;
        }

        public ICommand ShowNodeWithOutgoingCommand
        {
            get;
            private set;
        }

        public ICommand GoToEdgeSourceCommand
        {
            get;
            private set;
        }

        public ICommand GoToEdgeTargetCommand
        {
            get;
            private set;
        }

        protected override void OnModelPropertyChanged( string propertyName )
        {
            OnPropertyChanged( propertyName );

            if( propertyName == "Presentation" )
            {
                InvalidateLayoutCommand.RaiseCanExecuteChanged();
                PrintGraphCommand.RaiseCanExecuteChanged();
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
            get;
            set;
        }
    }
}
