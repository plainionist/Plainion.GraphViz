using System;
using System.Collections.Specialized;
using System.ComponentModel;
using System.ComponentModel.Composition;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows.Data;
using System.Windows.Input;
using Plainion.GraphViz.Infrastructure.ViewModel;
using Plainion.GraphViz.Presentation;
using Plainion.Prism.Mvvm;
using Prism.Commands;

namespace Plainion.GraphViz.Modules.Analysis
{
    [Export(typeof(NodeMasksEditorModel))]
    class NodeMasksEditorModel : ViewModelBase
    {
        private string myFilter;
        private bool myFilterOnId;
        private bool myIgnoreFolding;
        private bool myPreviewVisibleNodesOnly;
        private ICollectionView myPreviewNodes;
        private NodeWithCaption mySelectedPreviewItem;
        private IGraphPresentation myPresentation;

        public NodeMasksEditorModel()
        {
            myPreviewVisibleNodesOnly = true;

            AddCommand = new DelegateCommand(OnAdd);
            MouseDownCommand = new DelegateCommand<MouseButtonEventArgs>(OnMouseDown);
        }

        public ICommand MouseDownCommand { get; private set; }

        private void OnMouseDown(MouseButtonEventArgs args)
        {
            if (args.ClickCount == 2)
            {
                Filter = SelectedPreviewItem.DisplayText;
            }
        }

        private void OnAdd()
        {
            var regex = new Regex(Filter.ToLower(), RegexOptions.IgnoreCase);

            var graph = myIgnoreFolding ? myPresentation.Graph : myPresentation.GetModule<ITransformationModule>().Graph;
            var matchedNodes = graph.Nodes
                .Where(n => myFilterOnId ? regex.IsMatch(n.Id) : regex.IsMatch(myPresentation.GetPropertySetFor<Caption>().Get(n.Id).DisplayText));

            // TODO: should we have default "hide" really?
            var mask = new NodeMask();
            mask.IsShowMask = false;
            mask.Set(matchedNodes);
            mask.Label = string.Format("Pattern '{0}'", Filter);

            var module = myPresentation.GetModule<INodeMaskModule>();
            module.Push(mask);

            Filter = null;
        }

        public NodeWithCaption SelectedPreviewItem
        {
            get { return mySelectedPreviewItem; }
            set { SetProperty(ref mySelectedPreviewItem, value); }
        }

        public ICommand AddCommand { get; private set; }

        protected override void OnModelPropertyChanged(string propertyName)
        {
            if (propertyName == "Presentation")
            {
                if (myPresentation == Model.Presentation)
                {
                    return;
                }

                if (myPresentation != null)
                {
                    myPresentation.GetModule<INodeMaskModule>().CollectionChanged -= OnMasksChanged;
                    myPresentation.GraphVisibilityChanged -= OnGraphVisibilityChanged;
                }

                myPresentation = Model.Presentation;

                Filter = null;

                if (myPresentation != null)
                {
                    myPresentation.GetModule<INodeMaskModule>().CollectionChanged += OnMasksChanged;
                    myPresentation.GraphVisibilityChanged += OnGraphVisibilityChanged;

                    OnGraphVisibilityChanged(null, EventArgs.Empty);
                }

                myPreviewNodes = null;
                PreviewNodes.Refresh();
            }
        }

        private void OnMasksChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            PreviewNodes.Refresh();
        }

        private void OnGraphVisibilityChanged(object sender, EventArgs e)
        {
            // AddRange produces tons of notifications -> too expensive
            myPreviewNodes = null;
            RaisePropertyChanged(nameof(PreviewNodes));
        }

        public string Filter
        {
            get { return myFilter; }
            set
            {
                if (SetProperty(ref myFilter, value))
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
                if (SetProperty(ref myFilterOnId, value))
                {
                    ClearErrors();
                    PreviewNodes.Refresh();
                }
            }
        }

        public bool IgnoreFolding
        {
            get { return myIgnoreFolding; }
            set
            {
                if (SetProperty(ref myIgnoreFolding, value))
                {
                    ClearErrors();
                    // we have to completely rebuild the collection
                    myPreviewNodes = null;
                    PreviewNodes.Refresh();
                }
            }
        }

        public bool PreviewVisibleNodesOnly
        {
            get { return myPreviewVisibleNodesOnly; }
            set
            {
                if (SetProperty(ref myPreviewVisibleNodesOnly, value))
                {
                    PreviewNodes.Refresh();
                }
            }
        }

        public ICollectionView PreviewNodes
        {
            get
            {
                if (myPreviewNodes == null && myPresentation != null)
                {
                    var captionModule = myPresentation.GetPropertySetFor<Caption>();

                    var graph = myIgnoreFolding ? myPresentation.Graph : myPresentation.GetModule<ITransformationModule>().Graph;
                    var nodes = graph.Nodes
                        .Select(n => new NodeWithCaption(n, myFilterOnId ? n.Id : captionModule.Get(n.Id).DisplayText));

                    myPreviewNodes = CollectionViewSource.GetDefaultView(nodes);
                    myPreviewNodes.Filter = FilterPreview;
                    myPreviewNodes.SortDescriptions.Add(new SortDescription("DisplayText", ListSortDirection.Ascending));

                    RaisePropertyChanged(nameof(PreviewNodes));
                }
                return myPreviewNodes;
            }
        }

        private bool FilterPreview(object item)
        {
            if (GetErrors("Filters").OfType<object>().Any())
            {
                return true;
            }

            var node = (NodeWithCaption)item;

            if (myPreviewVisibleNodesOnly && !myPresentation.Picking.Pick(node.Node))
            {
                return false;
            }

            if (string.IsNullOrEmpty(Filter))
            {
                return true;
            }

            var pattern = Filter.ToLower();

            try
            {
                var regEx = new Regex(pattern, RegexOptions.IgnoreCase);

                return regEx.IsMatch(node.DisplayText);
            }
            catch
            {
                SetError(ValidationFailure.Error, "Filter");
                return true;
            }
        }
    }
}
