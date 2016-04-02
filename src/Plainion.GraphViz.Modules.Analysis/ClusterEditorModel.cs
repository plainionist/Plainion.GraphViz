using System.Collections.Specialized;
using System.ComponentModel;
using System.ComponentModel.Composition;
using System.Diagnostics;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows.Data;
using System.Windows.Input;
using Microsoft.Practices.Prism.Commands;
using Plainion.Collections;
using Plainion.GraphViz.Infrastructure.ViewModel;
using Plainion.GraphViz.Modules.Analysis.Services;
using Plainion.GraphViz.Presentation;
using Plainion.Prism.Mvvm;
using Plainion.Windows.Controls.Tree;

namespace Plainion.GraphViz.Modules.Analysis
{
    [Export( typeof( ClusterEditorModel ) )]
    class ClusterEditorModel : ViewModelBase
    {
        private string myFilter;
        private bool myFilterOnId;
        private bool myPreviewVisibleNodesOnly;
        private ICollectionView myPreviewNodes;
        private NodeWithCaption mySelectedPreviewItem;
        private IGraphPresentation myPresentation;
        private INodeMasksPersistanceService myPersistanceService;
        private DragDropBehavior myDragDropBehavior;

        [ImportingConstructor]
        public ClusterEditorModel( INodeMasksPersistanceService persistancyService )
        {
            myPersistanceService = persistancyService;

            myPreviewVisibleNodesOnly = true;

            AddCommand = new DelegateCommand( OnAdd );
            MouseDownCommand = new DelegateCommand<MouseButtonEventArgs>( OnMouseDown );

            Root = new Node();

            foreach( var process in Process.GetProcesses() )
            {
                var processNode = new Node() { Parent = Root, Id = process.Id.ToString(), Name = process.ProcessName };
                Root.Children.Add( processNode );

                processNode.Children.AddRange( process.Threads
                    .OfType<ProcessThread>()
                    .Select( t => new Node { Parent = processNode, Id = t.Id.ToString(), Name = "unknown" } ) );
            }

            myDragDropBehavior = new DragDropBehavior( Root );
            DropCommand = new DelegateCommand<NodeDropRequest>( myDragDropBehavior.ApplyDrop );
        }

        public Node Root { get; private set; }

        public ICommand DropCommand { get; private set; }
        
        public ICommand MouseDownCommand { get; private set; }

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
                .Where( n => myFilterOnId ? regex.IsMatch( n.Id ) : regex.IsMatch( myPresentation.GetPropertySetFor<Caption>().Get( n.Id ).DisplayText ) );

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

        public ICommand AddCommand { get; private set; }

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

        public bool FilterOnId
        {
            get { return myFilterOnId; }
            set
            {
                if( SetProperty( ref myFilterOnId, value ) )
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
                        .Select( n => new NodeWithCaption( n, myFilterOnId ? n.Id : captionModule.Get( n.Id ).DisplayText ) );

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
