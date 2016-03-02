using System;
using System.ComponentModel.Composition;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Markup;
using System.Windows.Media;
using System.Xml;
using ICSharpCode.AvalonEdit.Document;
using Microsoft.Practices.Prism.Commands;
using Microsoft.Practices.Prism.Interactivity.InteractionRequest;
using Plainion.GraphViz.Infrastructure.Services;
using Plainion.GraphViz.Infrastructure.ViewModel;
using Plainion.GraphViz.Modules.Reflection.Analysis.Packaging.Spec;
using Plainion.GraphViz.Modules.Reflection.Services;
using Plainion.GraphViz.Modules.Reflection.Services.Framework;
using Plainion.GraphViz.Presentation;
using Plainion.Prism.Interactivity.InteractionRequest;

namespace Plainion.GraphViz.Modules.Reflection.Analysis.Packaging
{
    [Export(typeof(PackagingGraphBuilderViewModel))]
    public class PackagingGraphBuilderViewModel : ViewModelBase
    {
        private int myProgress;
        private bool myIsReady;
        private Action myCancelBackgroundProcessing;
        //private IInspectorHandle<PackagingGraphInspector> myPackagingGraphInspector;
        private TextDocument myDocument;
        private SystemPackaging myPackagingSpec;

        public PackagingGraphBuilderViewModel()
        {
            Document = new TextDocument();

            CreateGraphCommand = new DelegateCommand(CreateGraph, () => IsReady);
            CancelCommand = new DelegateCommand(() => myCancelBackgroundProcessing(), () => !IsReady);

            ClosedCommand = new DelegateCommand(OnClosed);

            OpenCommand = new DelegateCommand(OnOpen, () => IsReady);
            OpenFileRequest = new InteractionRequest<OpenFileDialogNotification>();

            IsReady = true;
        }

        public TextDocument Document
        {
            get { return myDocument; }
            set { SetProperty(ref myDocument, value); }
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

        private void CreateGraph()
        {
            //IsReady = false;

            using (var reader = Document.CreateReader())
            {
                myPackagingSpec = (SystemPackaging)XamlReader.Load(XmlReader.Create(reader));
            }

            Save();

            var output = Path.GetTempFileName() + ".dot";

            if (File.Exists(output))
            {
                File.Delete(output);
            }

            var executable = Path.Combine(Path.GetDirectoryName(GetType().Assembly.Location), "Plainion.Graphviz.Pioneer.exe");
            var pioneer = Process.Start(executable, "-o " + output + " " + Document.FileName);
            pioneer.WaitForExit();

            if (pioneer.ExitCode == 0)
            {
                DocumentLoader.Load(output);
            }

            //InspectionService.UpdateInspectorOnDemand(ref myPackagingGraphInspector, Path.GetDirectoryName(AssemblyToAnalyseLocation));

            //myPackagingGraphInspector.Value.IgnoreDotNetTypes = IgnoreDotNetTypes;
            //myPackagingGraphInspector.Value.AssemblyLocation = AssemblyToAnalyseLocation;
            //myPackagingGraphInspector.Value.SelectedType = TypeToAnalyse;

            //myCancelBackgroundProcessing = InspectionService.RunAsync(myPackagingGraphInspector.Value, v => ProgressValue = v, OnPackagingGraphCompleted);
        }

        internal void OnClosed()
        {
            Save();
            Document.Text = string.Empty;

            //if (myCancelBackgroundProcessing != null)
            //{
            //    myCancelBackgroundProcessing();
            //}

            //InspectionService.DestroyInspectorOnDemand(ref myPackagingGraphInspector);

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

        private void OnPackagingGraphCompleted(TypeRelationshipDocument document)
        {
            try
            {
                if (document == null)
                {
                    return;
                }

                if (!document.Graph.Nodes.Any())
                {
                    MessageBox.Show("No nodes found");
                    return;
                }

                var presentation = PresentationCreationService.CreatePresentation(myPackagingSpec.AssemblyRoot);

                var captionModule = presentation.GetPropertySetFor<Caption>();
                var edgeStyleModule = presentation.GetPropertySetFor<EdgeStyle>();

                presentation.Graph = document.Graph;

                foreach (var desc in document.Descriptors)
                {
                    captionModule.Add(new Caption(desc.Id, desc.Name));
                }

                foreach (var entry in document.EdgeTypes)
                {
                    edgeStyleModule.Add(new EdgeStyle(entry.Key)
                    {
                        Color = entry.Value == EdgeType.DerivesFrom ? Brushes.Black : Brushes.Blue
                    });
                }

                if (document.FailedItems.Any())
                {
                    foreach (var item in document.FailedItems)
                    {
                        var sb = new StringBuilder();
                        sb.AppendLine("Loading failed");
                        sb.AppendFormat("Item: {0}", item.Item);
                        sb.AppendLine();
                        sb.AppendFormat("Reason: {0}", item.FailureReason);
                        StatusMessageService.Publish(new StatusMessage(sb.ToString()));
                    }
                }

                Model.Presentation = presentation;
            }
            finally
            {
                myCancelBackgroundProcessing = null;
                ProgressValue = 0;
                IsReady = true;
            }
        }
    }
}
