using System;
using System.Collections.Specialized;
using System.ComponentModel.Composition;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using System.Windows.Threading;
using Prism.Commands;
using Prism.Interactivity.InteractionRequest;
using Plainion.GraphViz.Algorithms;
using Plainion.GraphViz.Dot;
using Plainion.GraphViz.Infrastructure.Services;
using Plainion.GraphViz.Infrastructure.ViewModel;
using Plainion.GraphViz.Presentation;
using Plainion.GraphViz.Viewer.Services;
using Plainion.Windows;
using Plainion.Windows.Interactivity.DragDrop;

namespace Plainion.GraphViz.Viewer
{
    [Export(typeof(ShellViewModel))]
    public class ShellViewModel : ViewModelBase, IDropable
    {
        private IGraphPresentation myPresentation;
        private IStatusMessageService myStatusMessageService;
        private ConfigurationService myConfigurationService;
        private LayoutAlgorithm myLayoutAlgorithm;

        [ImportingConstructor]
        public ShellViewModel(IStatusMessageService statusMessageService, ConfigurationService configService)
        {
            myStatusMessageService = statusMessageService;
            myStatusMessageService.Messages.CollectionChanged += OnStatusMessagesChanged;

            myConfigurationService = configService;

            NodeMasksEditorRequest = new InteractionRequest<INotification>();
            OpenFilterEditor = new DelegateCommand(OnOpenFilterEditor);

            ClusterEditorRequest = new InteractionRequest<INotification>();
            OpenClusterEditor = new DelegateCommand(OnOpenClusterEditor);

            SettingsEditorRequest = new InteractionRequest<IConfirmation>();
            OpenSettingsEditor = new DelegateCommand(OnOpenSettingsEditor);

            ShowStatusMessagesRequest = new InteractionRequest<INotification>();
            ShowStatusMessagesCommand = new DelegateCommand(ShowStatusMessages);

            myConfigurationService.ConfigChanged += OnConfigChanged;
        }

        [Import(AllowDefault = true)]
        public IDocumentLoader DocumentLoader { get; set; }

        private void OnConfigChanged(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(myConfigurationService.Config.DotToolsHome) || !File.Exists(Path.Combine(myConfigurationService.Config.DotToolsHome, "dot.exe")))
            {
                myConfigurationService.Config.DotToolsHome = Path.GetDirectoryName(GetType().Assembly.Location);
            }

            if (File.Exists(Path.Combine(myConfigurationService.Config.DotToolsHome, "dot.exe")))
            {
                Model.LayoutEngine = new DotToolLayoutEngine(new DotToDotPlainConverter(myConfigurationService.Config.DotToolsHome));
            }
            else
            {
                // http://stackoverflow.com/questions/13026826/execute-command-after-view-is-loaded-wpf-mvvm
                Application.Current.Dispatcher.BeginInvoke(DispatcherPriority.ApplicationIdle, new Action(() =>
              {
                  // DotToolHome not set -> open settings editor
                  OpenSettingsEditor.Execute(null);
              }));
            }
        }

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

        protected override void OnModelPropertyChanged(string propertyName)
        {
            if (propertyName == "Model")
            {
                OnConfigChanged(this, EventArgs.Empty);
            }
            else if (propertyName == "Presentation")
            {
                if (myPresentation == Model.Presentation)
                {
                    return;
                }

                myPresentation = Model.Presentation;

                myPresentation.GetModule<INodeMaskModule>().AutoHideAllNodesForShowMasks = true;

                if (myPresentation.Graph.Nodes.Count() > DotToolLayoutEngine.FastRenderingNodeCountLimit)
                {
                    new ChangeClusterFolding(myPresentation)
                        .FoldUnfoldAllClusters();
                }

                var graphLayoutModule = myPresentation.GetModule<IGraphLayoutModule>();
                PropertyBinding.Bind(() => LayoutAlgorithm, () => graphLayoutModule.Algorithm);

                RaisePropertyChanged(nameof(IsEnabled));
            }
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
