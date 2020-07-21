using System;
using System.Collections.Specialized;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using Plainion.GraphViz.Dot;
using Plainion.GraphViz.Infrastructure.Services;
using Plainion.GraphViz.Infrastructure.ViewModel;
using Plainion.GraphViz.Presentation;
using Plainion.Windows;
using Plainion.Windows.Interactivity.DragDrop;
using Prism.Commands;
using Prism.Interactivity.InteractionRequest;
using Unity;

namespace Plainion.GraphViz.Viewer
{
    class ShellViewModel : ViewModelBase, IDropable
    {
        private IGraphPresentation myPresentation;
        private IStatusMessageService myStatusMessageService;
        private LayoutAlgorithm myLayoutAlgorithm;

        public ShellViewModel(IStatusMessageService statusMessageService, IDomainModel model)
            : base(model)
        {
            myStatusMessageService = statusMessageService;
            myStatusMessageService.Messages.CollectionChanged += OnStatusMessagesChanged;

            NodeMasksEditorRequest = new InteractionRequest<INotification>();
            OpenFilterEditor = new DelegateCommand(OnOpenFilterEditor);

            ClusterEditorRequest = new InteractionRequest<INotification>();
            OpenClusterEditor = new DelegateCommand(OnOpenClusterEditor);

            BookmarksRequest = new InteractionRequest<INotification>();
            OpenBookmarks = new DelegateCommand(OnOpenBookmarks);

            SettingsEditorRequest = new InteractionRequest<IConfirmation>();
            OpenSettingsEditor = new DelegateCommand(OnOpenSettingsEditor);

            ShowStatusMessagesRequest = new InteractionRequest<INotification>();
            ShowStatusMessagesCommand = new DelegateCommand(ShowStatusMessages);

            var args = Environment.GetCommandLineArgs();
            if (args.Length > 1)
            {
                // need to wait until initialization of UI is done
                Application.Current.Dispatcher.BeginInvoke(new Action(() => DocumentLoader.Load(args[1])));
            }
        }

        [OptionalDependency]
        public IDocumentLoader DocumentLoader { get; set; }

        private void OnOpenSettingsEditor()
        {
            var notification = new Confirmation();
            notification.Title = "Settings";

            SettingsEditorRequest.Raise(notification, c =>
               {
                   if (c.Confirmed && myPresentation != null)
                   {
                       myPresentation.InvalidateLayout();
                   }
               });
        }

        public ICommand OpenFilterEditor { get; private set; }

        private void OnOpenFilterEditor()
        {
            var notification = new Notification();
            notification.Title = "Filters";

            NodeMasksEditorRequest.Raise(notification, c => { });
        }

        public InteractionRequest<INotification> NodeMasksEditorRequest { get; private set; }

        public ICommand OpenClusterEditor { get; private set; }

        private void OnOpenClusterEditor()
        {
            var notification = new Notification();
            notification.Title = "Clusters";

            ClusterEditorRequest.Raise(notification, c => { });
        }

        public InteractionRequest<INotification> ClusterEditorRequest { get; private set; }

        public ICommand OpenBookmarks { get; private set; }

        private void OnOpenBookmarks()
        {
            var notification = new Notification();
            notification.Title = "Bookmarks";

            BookmarksRequest.Raise(notification, c => { });
        }

        public InteractionRequest<INotification> BookmarksRequest { get; private set; }

        public ICommand OpenSettingsEditor { get; private set; }

        public InteractionRequest<IConfirmation> SettingsEditorRequest { get; private set; }

        public InteractionRequest<INotification> ShowStatusMessagesRequest { get; private set; }

        private void ShowStatusMessages()
        {
            var notification = new Notification();
            notification.Title = "Status messages";

            ShowStatusMessagesRequest.Raise(notification, n => { });
        }

        private void OnStatusMessagesChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            RaisePropertyChanged(nameof(StatusBarVisibility));
        }

        public bool IsEnabled
        {
            get { return myPresentation != null; }
        }

        protected override void OnPresentationChanged()
        {
            if (myPresentation == Model.Presentation)
            {
                return;
            }

            myPresentation = Model.Presentation;

            if (myPresentation.Graph.Nodes.Count() > DotToolLayoutEngine.FastRenderingNodeCountLimit)
            {
                myPresentation.ToogleFoldingOfVisibleClusters();
            }

            var graphLayoutModule = myPresentation.GetModule<IGraphLayoutModule>();
            graphLayoutModule.Algorithm = LayoutAlgorithm;
            PropertyBinding.Bind(() => LayoutAlgorithm, () => graphLayoutModule.Algorithm);

            RaisePropertyChanged(nameof(IsEnabled));
        }

        public Visibility StatusBarVisibility
        {
            get { return myStatusMessageService.Messages.Any() ? Visibility.Visible : Visibility.Hidden; }
        }

        public ICommand ShowStatusMessagesCommand { get; private set; }

        string IDropable.DataFormat
        {
            get { return DataFormats.FileDrop; }
        }

        bool IDropable.IsDropAllowed(object data, DropLocation location)
        {
            return DocumentLoader != null && DocumentLoader.CanLoad(((string[])data).First());
        }

        void IDropable.Drop(object data, DropLocation location)
        {
            DocumentLoader.Load(((string[])data).First());
        }

        public LayoutAlgorithm LayoutAlgorithm
        {
            get { return myLayoutAlgorithm; }
            set
            {
                if (SetProperty(ref myLayoutAlgorithm, value))
                {
                    myPresentation.InvalidateLayout();
                }
            }
        }
    }
}
