using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Threading;
using Akka.Actor;
using Akka.Configuration;
using ICSharpCode.AvalonEdit.Document;
using Microsoft.Practices.Prism.Commands;
using Microsoft.Practices.Prism.Interactivity.InteractionRequest;
using Plainion.GraphViz.Infrastructure.Services;
using Plainion.GraphViz.Infrastructure.ViewModel;
using Plainion.GraphViz.Modules.Reflection.Analysis.Packaging.Actors;
using Plainion.GraphViz.Modules.Reflection.Analysis.Packaging.Spec;
using Plainion.GraphViz.Modules.Reflection.Services;
using Plainion.Prism.Interactivity.InteractionRequest;

namespace Plainion.GraphViz.Modules.Reflection.Analysis.Packaging
{
    [Export(typeof(PackagingGraphBuilderViewModel))]
    public class PackagingGraphBuilderViewModel : ViewModelBase
    {
        private int myProgress;
        private bool myIsReady;
        private TextDocument myDocument;
        private IEnumerable<KeywordCompletionData> myCompletionData;
        private ActorSystem mySystem;
        private IActorRef myActor;

        public PackagingGraphBuilderViewModel()
        {
            Document = new TextDocument();

            CreateGraphCommand = new DelegateCommand(OnCreateGraph, () => IsReady);
            CancelCommand = new DelegateCommand(OnCancel, () => !IsReady);

            ClosedCommand = new DelegateCommand(OnClosed);

            OpenCommand = new DelegateCommand(OnOpen, () => IsReady);
            OpenFileRequest = new InteractionRequest<OpenFileDialogNotification>();

            myCompletionData = GetType().Assembly.GetTypes()
                .Where(t => t.Namespace == typeof(SystemPackaging).Namespace)
                .Where(t => !t.IsAbstract)
                .Where(t => t.GetCustomAttribute(typeof(CompilerGeneratedAttribute), true) == null)
                .Select(t => new KeywordCompletionData(t))
                .ToList();

            IsReady = true;
        }

        public TextDocument Document
        {
            get { return myDocument; }
            set { SetProperty(ref myDocument, value); }
        }

        public IEnumerable<KeywordCompletionData> CompletionData
        {
            get { return myCompletionData; }
            set { SetProperty(ref myCompletionData, value); }
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
                    OpenCommand.RaiseCanExecuteChanged();
                }
            }
        }

        [Import]
        public IPresentationCreationService PresentationCreationService { get; set; }

        [Import]
        public IDocumentLoader DocumentLoader { get; set; }

        [Import]
        public IStatusMessageService StatusMessageService { get; set; }

        [Import]
        public AssemblyInspectionService InspectionService { get; set; }

        public DelegateCommand CreateGraphCommand { get; private set; }

        public DelegateCommand CancelCommand { get; private set; }

        public DelegateCommand OpenCommand { get; private set; }

        public DelegateCommand ClosedCommand { get; private set; }

        public InteractionRequest<OpenFileDialogNotification> OpenFileRequest { get; private set; }

        private void OnOpen()
        {
            var notification = new OpenFileDialogNotification();
            notification.RestoreDirectory = true;
            notification.Filter = "Packaging Spec (*.xaml)|*.xaml";
            notification.FilterIndex = 0;
            notification.CheckFileExists = false;

            OpenFileRequest.Raise(notification,
                n =>
                {
                    if (n.Confirmed)
                    {
                        if (File.Exists(n.FileName))
                        {
                            Document.Text = File.ReadAllText(n.FileName);
                        }
                        else
                        {
                            using (var stream = GetType().Assembly.GetManifestResourceStream("Plainion.GraphViz.Modules.Reflection.Resources.SystemPackagingTemplate.xaml"))
                            {
                                using (var reader = new StreamReader(stream))
                                {
                                    Document.Text = reader.ReadToEnd();
                                }
                            }
                        }
                        Document.FileName = n.FileName;
                    }
                });
        }

        private async void OnCreateGraph()
        {
            IsReady = false;

            var request = new GraphBuildRequest
            {
                Spec = Document.Text,
                OutputFile = Path.GetTempFileName() + ".dot"
            };

            var config = ConfigurationFactory.ParseString(@"
                akka {
                    actor {
                        provider = ""Akka.Remote.RemoteActorRefProvider, Akka.Remote""
                    }
                    remote {
                        helios.tcp {
                            port = 0
                            hostname = localhost
                        }
                    }
                }
                ");

            mySystem = ActorSystem.Create("CodeInspectionClient", config);

            var executable = Path.Combine(Path.GetDirectoryName(GetType().Assembly.Location), "Plainion.Graphviz.Pioneer.exe");
            var pioneer = Process.Start(executable, "-SAS");

            myActor = await mySystem.ActorSelection("akka.tcp://CodeInspection@localhost:2525/user/PackagingDependencies")
                .ResolveOne(TimeSpan.FromSeconds(5));

            var response = await myActor.Ask(request);

            if (!(response is Failure))
            {
                DocumentLoader.Load((string)response);
            }

            var ignore = mySystem.Terminate();

            pioneer.Kill();

            IsReady = true;
        }

        private void OnCancel()
        {
            myActor.Tell(new Cancel());
            IsReady = true;
        }

        internal void OnClosed()
        {
            Save();
            Document.Text = string.Empty;

            if (myActor != null)
            {
                myActor.Tell(new Cancel());
            }

            IsReady = true;
        }

        private void Save()
        {
            File.WriteAllText(Document.FileName, Document.Text);
        }

        protected override void OnModelPropertyChanged(string propertyName)
        {
        }

        public int ProgressValue
        {
            get { return myProgress; }
            set { SetProperty(ref myProgress, value); }
        }
    }
}
