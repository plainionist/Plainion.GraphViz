using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel.Composition;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using System.Windows;
using System.Windows.Media;
using ICSharpCode.AvalonEdit.Document;
using Microsoft.Practices.Prism.Commands;
using Microsoft.Practices.Prism.Interactivity.InteractionRequest;
using Plainion.Collections;
using Plainion.GraphViz.Infrastructure.Services;
using Plainion.GraphViz.Infrastructure.ViewModel;
using Plainion.GraphViz.Model;
using Plainion.GraphViz.Modules.CodeInspection.Packaging.Services;
using Plainion.GraphViz.Modules.CodeInspection.Packaging.Spec;
using Plainion.GraphViz.Presentation;
using Plainion.Prism.Interactivity.InteractionRequest;
using Plainion.Windows.Editors.Xml;

namespace Plainion.GraphViz.Modules.CodeInspection.Packaging
{
    [Export(typeof(PackagingGraphBuilderViewModel))]
    class PackagingGraphBuilderViewModel : ViewModelBase
    {
        private int myProgress;
        private bool myIsReady;
        private TextDocument myDocument;
        private IEnumerable<ElementCompletionData> myCompletionData;
        private CancellationTokenSource myCTS;
        private bool myUsedTypesOnly;
        private bool myAllEdges;
        private bool myCreateClustersForNamespaces;
        private readonly GraphToSpecSynchronizer myGraphToSpecSynchronizer;

        public PackagingGraphBuilderViewModel()
        {
            Document = new TextDocument();
            Document.Changed += Document_Changed;

            Packages = new ObservableCollection<string>();

            CreateGraphCommand = new DelegateCommand(OnCreateGraph, () => IsReady && Packages.Count > 0);
            CancelCommand = new DelegateCommand(OnCancel, () => !IsReady);

            ClosedCommand = new DelegateCommand(OnClosed);

            OpenCommand = new DelegateCommand(OnOpen, () => IsReady);
            OpenFileRequest = new InteractionRequest<OpenFileDialogNotification>();

            myCompletionData = GetType().Assembly.GetTypes()
                .Where(t => t.Namespace == typeof(SystemPackaging).Namespace)
                .Where(t => !t.IsAbstract)
                .Where(t => t.GetCustomAttribute(typeof(CompilerGeneratedAttribute), true) == null)
                .Select(t => new ElementCompletionData(t))
                .ToList();

            UsedTypesOnly = true;

            myGraphToSpecSynchronizer = new GraphToSpecSynchronizer(
                () => SpecUtils.Deserialize(Document.Text),
                spec =>
                {
                    Document.Text = SpecUtils.Serialize(spec);
                    Save();
                });

            IsReady = true;
        }

        private void Document_Changed(object sender, DocumentChangeEventArgs e)
        {
            Packages.Clear();

            if (string.IsNullOrEmpty(Document.Text))
            {
                return;
            }

            try
            {
                var spec = SpecUtils.Deserialize(Document.Text);
                Packages.AddRange(spec.Packages.Select(p => p.Name));
            }
            catch (Exception ex)
            {
                StatusMessageService.Publish(new StatusMessage("ERROR:" + ex));
            }

            CreateGraphCommand.RaiseCanExecuteChanged();
        }

        public ObservableCollection<string> Packages { get; private set; }

        public IEnumerable<string> PackagesToAnalyze { get; set; }

        public TextDocument Document
        {
            get { return myDocument; }
            set { SetProperty(ref myDocument, value); }
        }

        public IEnumerable<ElementCompletionData> CompletionData
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
        public IStatusMessageService StatusMessageService { get; set; }

        [Import]
        public PackageAnalysisService AnalysisService { get; set; }

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
                            using (var reader = new StreamReader(n.FileName, true))
                            {
                                Document.Text = reader.ReadToEnd();
                            }
                        }
                        else
                        {
                            using (var stream = GetType().Assembly.GetManifestResourceStream("Plainion.GraphViz.Modules.CodeInspection.Resources.SystemPackagingTemplate.xaml"))
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

            Save();

            var request = new AnalysisRequest
            {
                Spec = Document.Text,
                PackagesToAnalyze = PackagesToAnalyze != null ? PackagesToAnalyze.ToArray() : null,
                OutputFile = Path.GetTempFileName(),
                UsedTypesOnly = UsedTypesOnly,
                AllEdges = AllEdges,
                CreateClustersForNamespaces = CreateClustersForNamespaces
            };

            try
            {
                myCTS = new CancellationTokenSource();

                var doc = await AnalysisService.Analyse(request, myCTS.Token);

                myCTS = null;

                if (doc != null)
                {
                    BuildGraph(doc);
                }
            }
            finally
            {
                IsReady = true;
            }
        }

        private void BuildGraph(AnalysisDocument response)
        {
            if (!response.Nodes.Any() && !response.Edges.Any())
            {
                MessageBox.Show("Neither nodes nor edges found");
                return;
            }

            var presentation = PresentationCreationService.CreatePresentation(Path.GetTempPath());

            var builder = new RelaxedGraphBuilder();
            foreach (var edge in response.Edges)
            {
                builder.TryAddEdge(edge.Item1, edge.Item2);
            }

            foreach (var node in response.Nodes)
            {
                builder.TryAddNode(node);
            }

            foreach (var cluster in response.Clusters)
            {
                builder.TryAddCluster(cluster.Key, cluster.Value);
            }

            // add potentially empty clusters
            {
                var spec = SpecUtils.Deserialize(Document.Text);
                var emptyClusters = spec.Packages
                    .Where(p => PackagesToAnalyze == null || PackagesToAnalyze.Contains(p.Name))
                    .SelectMany(p => p.Clusters)
                    .Select(c => c.Name)
                    .Except(response.Clusters.Select(c => c.Key));

                foreach (var cluster in emptyClusters)
                {
                    builder.TryAddCluster(cluster, Enumerable.Empty<string>());
                }
            }

            presentation.Graph = builder.Graph;

            var captionModule = presentation.GetPropertySetFor<Caption>();
            foreach (var caption in response.Captions)
            {
                captionModule.Add(new Caption(caption.Key, caption.Value));
            }

            var converter = new BrushConverter();

            var nodeStyles = presentation.GetPropertySetFor<NodeStyle>();
            foreach (var style in response.NodeStyles)
            {
                var brush = (Brush)converter.ConvertFromString(style.Value);
                brush.Freeze();
                nodeStyles.Add(new NodeStyle(style.Key) { FillColor = brush });
            }

            var edgeStyles = presentation.GetPropertySetFor<EdgeStyle>();
            foreach (var style in response.EdgeStyles)
            {
                var brush = (Brush)converter.ConvertFromString(style.Value);
                brush.Freeze();
                edgeStyles.Add(new EdgeStyle(style.Key) { Color = brush });
            }

            Model.Presentation = presentation;

            // only synchronize our own presentation
            myGraphToSpecSynchronizer.Presentation = presentation;
        }

        private void OnCancel()
        {
            if (myCTS != null)
            {
                myCTS.Cancel();
            }

            IsReady = true;
        }

        internal void OnClosed()
        {
            Save();
            Document.Text = string.Empty;

            if (myCTS != null)
            {
                myCTS.Cancel();
            }

            IsReady = true;
        }

        private void Save()
        {
            if (Document.FileName != null)
            {
                using (var writer = new StreamWriter(Document.FileName, false, Encoding.UTF8))
                {
                    writer.WriteLine(Document.Text);
                }
            }
        }

        protected override void OnModelPropertyChanged(string propertyName)
        {
            if (propertyName == "Presentation")
            {
                // reset only!
                myGraphToSpecSynchronizer.Presentation = null;
            }
        }

        public int ProgressValue
        {
            get { return myProgress; }
            set { SetProperty(ref myProgress, value); }
        }

        public bool UsedTypesOnly
        {
            get { return myUsedTypesOnly; }
            set { SetProperty(ref myUsedTypesOnly, value); }
        }

        public bool AllEdges
        {
            get { return myAllEdges; }
            set { SetProperty(ref myAllEdges, value); }
        }

        public bool CreateClustersForNamespaces
        {
            get { return myCreateClustersForNamespaces; }
            set { SetProperty(ref myCreateClustersForNamespaces, value); }
        }
    }
}
