using System.Collections.Specialized;
using System.ComponentModel;
using System.ComponentModel.Composition;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows.Data;
using System.Windows.Input;
using Microsoft.Practices.Prism.Commands;
using Plainion.Collections;
using Plainion.GraphViz.Infrastructure.ViewModel;
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
        private ICollectionView myPreviewNodes;
        private NodeWithCaption mySelectedPreviewItem;
        private IGraphPresentation myPresentation;
        private DragDropBehavior myDragDropBehavior;

        [ImportingConstructor]
        public ClusterEditorModel()
        {
            AddCommand = new DelegateCommand( OnAdd );
            MouseDownCommand = new DelegateCommand<MouseButtonEventArgs>( OnMouseDown );

            Root = new ClusterTreeNode( null, null );

            myDragDropBehavior = new DragDropBehavior( Root );
            DropCommand = new DelegateCommand<NodeDropRequest>( myDragDropBehavior.ApplyDrop );
        }

        public ClusterTreeNode Root { get; private set; }

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
            // TODO:

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
                    myPresentation.GetModule<ITransformationModule>().CollectionChanged -= OnTransformationsChanged;
                }

                myPresentation = Model.Presentation;

                Filter = null;

                {
                    myPresentation.GetModule<ITransformationModule>().CollectionChanged += OnTransformationsChanged;
                }

                myPreviewNodes = null;
                PreviewNodes.Refresh();

                BuildTree();
            }
        }

        private void BuildTree()
        {
            Root.Children.Clear();

            var transformationModule = myPresentation.GetModule<ITransformationModule>();
            var captionModule = myPresentation.GetModule<ICaptionModule>();
            foreach( var cluster in transformationModule.Graph.Clusters )
            {
                var clusterNode = new ClusterTreeNode( myPresentation, null )
                {
                    Parent = Root,
                    Id = cluster.Id,
                    Caption = captionModule.Get( cluster.Id ).DisplayText,
                    IsExpanded = true
                };
                Root.Children.Add( clusterNode );

                clusterNode.Children.AddRange( cluster.Nodes
                    .Select( n => new ClusterTreeNode( myPresentation, n )
                    {
                        Parent = clusterNode,
                        Id = n.Id,
                        Caption = captionModule.Get( n.Id ).DisplayText
                    } ) );
            }
        }

        private void OnTransformationsChanged( object sender, NotifyCollectionChangedEventArgs e )
        {
            BuildTree();
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

            var transformationModule = myPresentation.GetModule<ITransformationModule>();
            if( !transformationModule.Graph.Clusters.Any( c => c.Nodes.Any( n => n.Id == node.Node.Id ) ) )
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
