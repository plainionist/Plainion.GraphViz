using System;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using Plainion.GraphViz.Dot;
using Plainion.GraphViz.Presentation;
using Plainion.GraphViz.Viewer.Abstractions.Services;
using Plainion.GraphViz.Viewer.Abstractions.ViewModel;
using Plainion.Prism.Interactivity.InteractionRequest;
using Plainion.Windows;
using Plainion.Windows.Interactivity.DragDrop;
using Prism.Commands;
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

            Themes.Naked.PropertyChanged += Naked_PropertyChanged;
        }

        private void Naked_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            RaisePropertyChanged(nameof(NakedVisibility));
        }

        public Visibility NakedVisibility => Themes.Naked.IsEnabled ? Visibility.Hidden : Visibility.Visible;
        
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

            // Automatically fold all clusters if more than 500 nodes (random bigger number)
            if (myPresentation.Graph.Nodes.Count() > 500)
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
