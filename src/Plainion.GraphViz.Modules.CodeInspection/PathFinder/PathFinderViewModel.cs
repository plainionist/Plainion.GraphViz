using System.ComponentModel.Composition;
using System.IO;
using System.Threading;
using System.Windows;
using Plainion.GraphViz.Infrastructure.Services;
using Plainion.GraphViz.Infrastructure.ViewModel;
using Plainion.GraphViz.Modules.CodeInspection.PathFinder.Actors;
using Plainion.Prism.Interactivity.InteractionRequest;
using Prism.Commands;
using Prism.Interactivity.InteractionRequest;

namespace Plainion.GraphViz.Modules.CodeInspection.PathFinder
{
    [Export]
    class PathFinderViewModel : ViewModelBase
    {
        private string myConfigFile;
        private bool myIsReady;
        private bool myAssemblyReferencesOnly;
        private CancellationTokenSource myCTS;
        private IPresentationCreationService myPresentationCreationService;
        private PathFinderClient myClient;
        private IDocumentLoader myDocumentLoader;

        [ImportingConstructor]
        public PathFinderViewModel(IPresentationCreationService presentationCreationService, IDocumentLoader documentLoader, PathFinderClient client)
        {
            myPresentationCreationService = presentationCreationService;
            myDocumentLoader = documentLoader;
            myClient = client;

            CreateGraphCommand = new DelegateCommand(CreateGraph, () => ConfigFile != null && IsReady);
            CancelCommand = new DelegateCommand(OnCancel, () => !IsReady);
            OpenConfigFileCommand = new DelegateCommand(OnOpenConfigFile, () => IsReady);
            ClosedCommand = new DelegateCommand(OnClosed);

            OpenFileRequest = new InteractionRequest<OpenFileDialogNotification>();

            IsReady = true;
            AssemblyReferencesOnly = false;
        }

        private void OnCancel()
        {
            if (myCTS != null)
            {
                myCTS.Cancel();
            }

            IsReady = true;
        }

        public bool IsReady
        {
            get { return myIsReady; }
            set
            {
                if (SetProperty(ref myIsReady, value))
                {
                    CreateGraphCommand.RaiseCanExecuteChanged();
                    CancelCommand.RaiseCanExecuteChanged();
                    OpenConfigFileCommand.RaiseCanExecuteChanged();
                }
            }
        }

        public DelegateCommand CreateGraphCommand { get; private set; }

        public DelegateCommand CancelCommand { get; private set; }

        public DelegateCommand OpenConfigFileCommand { get; private set; }

        public DelegateCommand ClosedCommand { get; private set; }

        public InteractionRequest<OpenFileDialogNotification> OpenFileRequest { get; private set; }

        private void OnOpenConfigFile()
        {
            var notification = new OpenFileDialogNotification();
            notification.RestoreDirectory = true;
            notification.Filter = "PathFinder config (*.json)|*.json";
            notification.FilterIndex = 0;

            OpenFileRequest.Raise(notification,
                n =>
                {
                    if (n.Confirmed)
                    {
                        ConfigFile = n.FileName;
                    }
                });
        }

        private async void CreateGraph()
        {
            IsReady = false;

            try
            {
                myCTS = new CancellationTokenSource();

                var dotFile = await myClient.AnalyzePathAsync(ConfigFile, AssemblyReferencesOnly, myCTS.Token);

                myCTS.Dispose();
                myCTS = null;

                if (dotFile != null)
                {
                    OnPathFound(dotFile);
                }
            }
            finally
            {
                IsReady = true;
            }
        }

        private void OnClosed()
        {
            ConfigFile = null;

            if (myCTS != null)
            {
                myCTS.Cancel();
            }

            IsReady = true;
        }

        public string ConfigFile
        {
            get { return myConfigFile; }
            set
            {
                if (SetProperty(ref myConfigFile, value))
                {
                    CreateGraphCommand.RaiseCanExecuteChanged();
                }
            }
        }

        public bool AssemblyReferencesOnly
        {
            get { return myAssemblyReferencesOnly; }
            set { SetProperty(ref myAssemblyReferencesOnly, value); }
        }

        private void OnPathFound(string dotFile)
        {
            if (!File.Exists(dotFile))
            {
                MessageBox.Show("No paths found");
                return;
            }

            myDocumentLoader.Load(dotFile);
        }
    }
}
