using System.Collections.Specialized;
using System.ComponentModel;
using System.ComponentModel.Composition;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows.Data;
using System.Windows.Input;
using Plainion.GraphViz.Infrastructure.ViewModel;
using Plainion.GraphViz.Modules.Analysis.Services;
using Plainion.GraphViz.Presentation;
using Microsoft.Practices.Prism.Commands;
using Microsoft.Practices.Prism.Interactivity.InteractionRequest;
using Plainion.Prism.Interactivity.InteractionRequest;
using Plainion.Prism.Mvvm;

namespace Plainion.GraphViz.Modules.Analysis
{
    [Export( typeof( NodeMasksEditorModel ) )]
    public class NodeMasksEditorModel : ViewModelBase
    {
        private string myFilter;
        private bool myPreviewVisibleNodesOnly;
        private ICollectionView myPreviewNodes;
        private NodeWithCaption mySelectedPreviewItem;
        private IGraphPresentation myPresentation;
        private INodeMasksPersistanceService myPersistanceService;

        [ImportingConstructor]
        public NodeMasksEditorModel( INodeMasksPersistanceService persistancyService )
        {
            myPersistanceService = persistancyService;

            myPreviewVisibleNodesOnly = true;

            AddCommand = new DelegateCommand( OnAdd );
            MouseDownCommand = new DelegateCommand<MouseButtonEventArgs>( OnMouseDown );

            LoadMasksCommand = new DelegateCommand( LoadMasks );
            OpenFileRequest = new InteractionRequest<OpenFileDialogNotification>();

            SaveMasksCommand = new DelegateCommand( SaveMasks );
            SaveFileRequest = new InteractionRequest<SaveFileDialogNotification>();
        }

        public DelegateCommand LoadMasksCommand { get; private set; }

        public InteractionRequest<OpenFileDialogNotification> OpenFileRequest { get; private set; }

        private void LoadMasks()
        {
            var notification = new OpenFileDialogNotification();
            notification.RestoreDirectory = true;
            notification.Filter = "GraphViz filter files (*.bgf)|*.bgf";
            notification.FilterIndex = 0;
            notification.DefaultExt = ".bgf";

            OpenFileRequest.Raise( notification,
                n =>
                {
                    if( n.Confirmed )
                    {
                        var masks = myPersistanceService.Load( n.FileName );

                        var module = myPresentation.GetModule<INodeMaskModule>();
                        var masksToRemove = module.Items.ToList();

                        // unfort. we have to add masks first and then remove the old ones because the system reacts on
                        // removal and in case of very huge graphs this causes performance issues because then the whole
                        // graph is picked instead of subset

                        // do reverse as we use push (stack)
                        foreach( var mask in masks.Reverse() )
                        {
                            module.Push( mask );
                        }

                        foreach( var mask in masksToRemove )
                        {
                            module.Remove( mask );
                        }

                    }
                } );
        }

        public DelegateCommand SaveMasksCommand { get; private set; }

        public InteractionRequest<SaveFileDialogNotification> SaveFileRequest { get; private set; }

        private void SaveMasks()
        {
            var notification = new SaveFileDialogNotification();
            notification.RestoreDirectory = true;
            notification.Filter = "GraphViz filter files (*.bgf)|*.bgf";
            notification.FilterIndex = 0;
            notification.DefaultExt = ".bgf";

            SaveFileRequest.Raise( notification,
                n =>
                {
                    if( n.Confirmed )
                    {
                        var module = myPresentation.GetModule<INodeMaskModule>();
                        myPersistanceService.Save( n.FileName, module.Items );
                    }
                } );
        }

        public ICommand MouseDownCommand
        {
            get;
            private set;
        }

        private void OnMouseDown( MouseButtonEventArgs args )
        {
            if( args.ClickCount == 2 )
            {
                Filter = SelectedPreviewItem.DisplayText;
            }
        }

        private void OnAdd()
        {
            var regex = new Regex( Filter.ToLower(), RegexOptions.IgnoreCase );

            var matchedNodes = myPresentation.Graph.Nodes
                .Where( n => regex.IsMatch( n.Id ) || regex.IsMatch( myPresentation.GetPropertySetFor<Caption>().Get( n.Id ).DisplayText ) );

            // TODO: should we have default "hide" really?
            var mask = new NodeMask();
            mask.IsShowMask = false;
            mask.Set( matchedNodes );
            mask.Label = string.Format( "Pattern '{0}'", Filter );

            var module = myPresentation.GetModule<INodeMaskModule>();

            module.Push( mask );

            Filter = null;
        }

        public NodeWithCaption SelectedPreviewItem
        {
            get { return mySelectedPreviewItem; }
            set { SetProperty( ref mySelectedPreviewItem, value ); }
        }

        public ICommand AddCommand
        {
            get;
            private set;
        }

        protected override void OnModelPropertyChanged( string propertyName )
        {
            if( propertyName == "Presentation" )
            {
                if( myPresentation == Model.Presentation )
                {
                    return;
                }

                if( myPresentation != null )
                {
                    myPresentation.GetModule<INodeMaskModule>().CollectionChanged -= OnMasksChanged;
                }

                myPresentation = Model.Presentation;

                Filter = null;

                {
                    myPresentation.GetModule<INodeMaskModule>().CollectionChanged += OnMasksChanged;
                }

                myPreviewNodes = null;
                PreviewNodes.Refresh();
            }
        }

        private void OnMasksChanged( object sender, NotifyCollectionChangedEventArgs e )
        {
            PreviewNodes.Refresh();
        }

        public string Filter
        {
            get { return myFilter; }
            set
            {
                if( SetProperty( ref myFilter, value ) )
                {
                    ClearErrors();
                    PreviewNodes.Refresh();
                }
            }
        }

        public bool PreviewVisibleNodesOnly
        {
            get { return myPreviewVisibleNodesOnly; }
            set
            {
                if( SetProperty( ref myPreviewVisibleNodesOnly, value ) )
                {
                    PreviewNodes.Refresh();
                }
            }
        }

        public ICollectionView PreviewNodes
        {
            get
            {
                if( myPreviewNodes == null && myPresentation != null )
                {
                    var captionModule = myPresentation.GetPropertySetFor<Caption>();

                    var nodes = myPresentation.Graph.Nodes
                         .Select( n => new NodeWithCaption( n, captionModule.Get( n.Id ).DisplayText ) );

                    myPreviewNodes = CollectionViewSource.GetDefaultView( nodes );
                    myPreviewNodes.Filter = FilterPreview;
                    myPreviewNodes.SortDescriptions.Add( new SortDescription( "DisplayText", ListSortDirection.Ascending ) );

                    OnPropertyChanged( "PreviewNodes" );
                }
                return myPreviewNodes;
            }
        }

        private bool FilterPreview( object item )
        {
            if( GetErrors( "Filters" ).OfType<object>().Any() )
            {
                return true;
            }

            var node = ( NodeWithCaption )item;

            if( myPreviewVisibleNodesOnly && !myPresentation.Picking.Pick( node.Node ) )
            {
                return false;
            }

            if( string.IsNullOrEmpty( Filter ) )
            {
                return true;
            }

            var pattern = Filter.ToLower();

            try
            {
                var regEx = new Regex( pattern, RegexOptions.IgnoreCase );

                return regEx.IsMatch( node.DisplayText );
            }
            catch
            {
                SetError( ValidationFailure.Error, "Filter" );
                return true;
            }
        }
    }
}
