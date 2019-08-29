using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel.Composition;
using System.Linq;
using System.Windows.Controls;
using System.Windows.Input;
using Plainion.GraphViz.Infrastructure;
using Plainion.GraphViz.Infrastructure.ViewModel;
using Plainion.GraphViz.Presentation;
using Prism.Commands;
using Prism.Events;

namespace Plainion.GraphViz.Modules.Analysis
{
    [Export(typeof(SearchBoxModel))]
    class SearchBoxModel : ViewModelBase
    {
        private IGraphPresentation myPresentation;
        private NodeWithCaption mySelectedItem;

        [ImportingConstructor]
        public SearchBoxModel(IEventAggregator eventAggregator)
        {
            VisibleNodes = new ObservableCollection<NodeWithCaption>();
            ItemFilter = OnFilterItem;

            SearchCommittedCommand = new DelegateCommand(() =>
            {
                if (SelectedItem != null)
                {
                    eventAggregator.GetEvent<NodeFocusedEvent>().Publish(SelectedItem.Node);
                }
            });
        }

        private IEnumerable<NodeWithCaption> GetVisibleNodes()
        {
            var captionModule = myPresentation.GetPropertySetFor<Caption>();
            var transformations = myPresentation.GetModule<ITransformationModule>();
            return transformations.Graph.Nodes
                .Where(n => myPresentation.Picking.Pick(n))
                .Select(n => new NodeWithCaption(n, captionModule.Get(n.Id).DisplayText));
        }

        protected override void OnPresentationChanged()
        {
            if (myPresentation == Model.Presentation)
            {
                return;
            }

            if (myPresentation != null)
            {
                myPresentation.GraphVisibilityChanged -= OnGraphVisibilityChanged;
            }

            myPresentation = Model.Presentation;

            if (myPresentation != null)
            {
                myPresentation.GraphVisibilityChanged += OnGraphVisibilityChanged;

                OnGraphVisibilityChanged(null, EventArgs.Empty);
            }

            RaisePropertyChanged(nameof(IsEnabled));
        }

        private void OnGraphVisibilityChanged(object sender, EventArgs e)
        {
            // AddRange produces tons of notifications -> too expensive
            VisibleNodes = new ObservableCollection<NodeWithCaption>(GetVisibleNodes());
            RaisePropertyChanged(nameof(VisibleNodes));
        }

        public bool IsEnabled
        {
            get { return myPresentation != null; }
        }

        public IEnumerable<NodeWithCaption> VisibleNodes { get; private set; }

        public AutoCompleteFilterPredicate<object> ItemFilter { get; private set; }

        private bool OnFilterItem(string search, object item)
        {
            if (string.IsNullOrEmpty(search))
            {
                return true;
            }

            var node = (NodeWithCaption)item;

            return node.DisplayText.Contains(search, StringComparison.OrdinalIgnoreCase);
        }

        public ICommand SearchCommittedCommand { get; private set; }

        public NodeWithCaption SelectedItem
        {
            get { return mySelectedItem; }
            set
            {
                if (mySelectedItem == value)
                {
                    return;
                }

                mySelectedItem = value;
                RaisePropertyChanged(nameof(SelectedItem));
            }
        }
    }
}
