using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel.Composition;
using System.Linq;
using System.Windows.Controls;
using System.Windows.Input;
using Plainion.Collections;
using Plainion.GraphViz.Infrastructure;
using Plainion.GraphViz.Infrastructure.ViewModel;
using Plainion.GraphViz.Presentation;
using Microsoft.Practices.Prism.Commands;
using Microsoft.Practices.Prism.PubSubEvents;

namespace Plainion.GraphViz.Modules.Analysis
{
    [Export( typeof( SearchBoxModel ) )]
    public class SearchBoxModel : ViewModelBase
    {
        private IGraphPresentation myPresentation;
        private NodeWithCaption mySelectedItem;

        [ImportingConstructor]
        public SearchBoxModel( IEventAggregator eventAggregator )
        {
            VisibleNodes = new ObservableCollection<NodeWithCaption>();
            ItemFilter = OnFilterItem;

            SearchCommittedCommand = new DelegateCommand( () => eventAggregator.GetEvent<NodeFocusedEvent>().Publish( SelectedItem.Node ) );
        }

        private void OnGraphVisibilityChanged( object sender, EventArgs e )
        {
            VisibleNodes.Clear();
            VisibleNodes.AddRange( GetVisibleNodes() );
        }

        private IEnumerable<NodeWithCaption> GetVisibleNodes()
        {
            var captionModule = myPresentation.GetPropertySetFor<Caption>();
            var transformations = myPresentation.GetModule<ITransformationModule>();
            return transformations.Graph.Nodes
                .Where( n => myPresentation.Picking.Pick( n ) )
                .Select( n => new NodeWithCaption( n, captionModule.Get( n.Id ).DisplayText ) );
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
                    myPresentation.GraphVisibilityChanged -= OnGraphVisibilityChanged;
                }

                myPresentation = Model.Presentation;

                if( myPresentation != null )
                {
                    myPresentation.GraphVisibilityChanged += OnGraphVisibilityChanged;

                    OnGraphVisibilityChanged( null, EventArgs.Empty );
                }

                OnPropertyChanged( "IsEnabled" );
            }
        }

        public bool IsEnabled
        {
            get { return myPresentation != null; }
        }

        public ObservableCollection<NodeWithCaption> VisibleNodes
        {
            get;
            private set;
        }

        public AutoCompleteFilterPredicate<object> ItemFilter { get; private set; }

        private bool OnFilterItem( string search, object item )
        {
            var node = ( NodeWithCaption )item;

            return node.DisplayText.ToLower().Contains( search.ToLower() );
        }

        public ICommand SearchCommittedCommand
        {
            get;
            private set;
        }

        public NodeWithCaption SelectedItem
        {
            get { return mySelectedItem; }
            set
            {
                if( mySelectedItem == value )
                {
                    return;
                }

                mySelectedItem = value;
                OnPropertyChanged( "SelectedItem" );
            }
        }
    }
}
