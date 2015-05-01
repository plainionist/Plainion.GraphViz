using System;
using System.Collections.Specialized;
using System.ComponentModel.Composition;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using Plainion.GraphViz.Dot;
using Plainion.GraphViz.Infrastructure.Services;
using Plainion.GraphViz.Infrastructure.ViewModel;
using Plainion.GraphViz.Presentation;
using Plainion.GraphViz.Viewer.Services;
using Microsoft.Practices.Prism.Commands;
using Microsoft.Practices.Prism.Interactivity.InteractionRequest;

namespace Plainion.GraphViz.Viewer
{
    [Export( typeof( ShellViewModel ) )]
    public class ShellViewModel : ViewModelBase
    {
        private IGraphPresentation myPresentation;
        private IStatusMessageService myStatusMessageService;
        private ConfigurationService myConfigurationService;

        [ImportingConstructor]
        public ShellViewModel( IStatusMessageService statusMessageService, ConfigurationService configService )
        {
            myStatusMessageService = statusMessageService;
            myStatusMessageService.Messages.CollectionChanged += OnStatusMessagesChanged;

            myConfigurationService = configService;

            NodeMasksEditorRequest = new InteractionRequest<INotification>();
            OpenFilterEditor = new DelegateCommand( OnOpenFilterEditor );

            SettingsEditorRequest = new InteractionRequest<IConfirmation>();
            OpenSettingsEditor = new DelegateCommand( OnOpenSettingsEditor );

            ShowStatusMessagesRequest = new InteractionRequest<INotification>();
            ShowStatusMessagesCommand = new DelegateCommand( ShowStatusMessages );

            myConfigurationService.ConfigChanged += OnConfigChanged;
        }

        void OnConfigChanged( object sender, EventArgs e )
        {
            Model.LayoutEngine = new DotToolLayoutEngine( new DotToDotPlainConverter( myConfigurationService.Config.DotToolsHome ) );
        }

        private void OnOpenSettingsEditor()
        {
            var notification = new Confirmation();
            notification.Title = "Settings";

            SettingsEditorRequest.Raise( notification, c =>
                {
                    if( c.Confirmed )
                    {
                        myPresentation.InvalidateLayout();
                    }
                } );
        }

        private void OnOpenFilterEditor()
        {
            var notification = new Notification();
            notification.Title = "Filters";

            NodeMasksEditorRequest.Raise( notification, c => { } );
        }

        public ICommand OpenFilterEditor
        {
            get;
            private set;
        }

        public ICommand OpenSettingsEditor
        {
            get;
            private set;
        }

        public InteractionRequest<INotification> NodeMasksEditorRequest { get; private set; }

        public InteractionRequest<IConfirmation> SettingsEditorRequest { get; private set; }

        public InteractionRequest<INotification> ShowStatusMessagesRequest { get; private set; }

        private void ShowStatusMessages()
        {
            var notification = new Notification();
            notification.Title = "Status messages";

            ShowStatusMessagesRequest.Raise( notification, n => { } );
        }

        private void OnStatusMessagesChanged( object sender, NotifyCollectionChangedEventArgs e )
        {
            OnPropertyChanged( "StatusBarVisibility" );
        }

        public bool IsEnabled
        {
            get
            {
                return myPresentation != null;
            }
        }

        protected override void OnModelPropertyChanged( string propertyName )
        {
            if( propertyName == "Model" )
            {
                OnConfigChanged( this, EventArgs.Empty );
            }
            else if( propertyName == "Presentation" )
            {
                if( myPresentation == Model.Presentation )
                {
                    return;
                }

                myPresentation = Model.Presentation;

                myPresentation.GetModule<INodeMaskModule>().AutoHideAllNodesForShowMasks = true;

                if( myPresentation.Graph.Nodes.Count() > 300 )
                {
                    var hideAllButOne = new NodeMask();
                    hideAllButOne.IsShowMask = true;
                    hideAllButOne.IsApplied = true;
                    hideAllButOne.Label = "Hide all but one node";

                    hideAllButOne.Set( myPresentation.Graph.Nodes.Take( 1 ) );

                    myPresentation.GetModule<INodeMaskModule>().Push( hideAllButOne );

                    OnOpenFilterEditor();
                }

                OnPropertyChanged( "IsEnabled" );
            }
        }

        public Visibility StatusBarVisibility
        {
            get { return myStatusMessageService.Messages.Any() ? Visibility.Visible : Visibility.Hidden; }
        }

        public ICommand ShowStatusMessagesCommand
        {
            get;
            private set;
        }
    }
}
