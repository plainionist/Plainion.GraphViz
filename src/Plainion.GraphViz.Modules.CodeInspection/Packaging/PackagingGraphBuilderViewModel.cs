
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using System.Windows;
using System.Windows.Media;
using ICSharpCode.AvalonEdit.Document;
using Plainion.Collections;
using Plainion.GraphViz.Infrastructure.Services;
using Plainion.GraphViz.Infrastructure.ViewModel;
using Plainion.GraphViz.Model;
using Plainion.GraphViz.Modules.CodeInspection.Packaging.Actors;
using Plainion.GraphViz.Modules.CodeInspection.Packaging.Analyzers;
using Plainion.GraphViz.Modules.CodeInspection.Packaging.Spec;
using Plainion.GraphViz.Presentation;
using Plainion.Prism.Interactivity.InteractionRequest;
using Plainion.Windows.Editors.Xml;
using Prism.Commands;

namespace Plainion.GraphViz.Modules.CodeInspection.Packaging
{
    class PackagingGraphBuilderViewModel : ViewModelBase
    {
        private bool myIsReady;
        private TextDocument myDocument;
        private IEnumerable<ElementCompletionData> myCompletionData;
        private CancellationTokenSource myCTS;
        private bool myUsedTypesOnly;
        private bool myCreateClustersForNamespaces;
        private readonly GraphToSpecSynchronizer myGraphToSpecSynchronizer;
        private readonly IPresentationCreationService myPresentationCreationService;
        private readonly IStatusMessageService myStatusMessageService;
        private readonly PackageAnalysisClient myAnalysisClient;
        private IDocumentLoader myDocumentLoader;
        private string myFileName;

        public PackagingGraphBuilderViewModel(IPresentationCreationService presentationCreationService, IStatusMessageService statusMessageService, PackageAnalysisClient analysisClient, IDocumentLoader documentLoader, IDomainModel model)
            : base(model)
        {
            myPresentationCreationService = presentationCreationService;
            myStatusMessageService = statusMessageService;
            myAnalysisClient = analysisClient;
            myDocumentLoader = documentLoader;

            Document = new TextDocument();
            SetInitialTemplate(Document);
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

        private void SetInitialTemplate(TextDocument document)
        {
            document.Text = 
                @"<SystemPackaging AssemblyRoot=""ASSEMBLY_ROOT_UNDEFINED"" NetFramework=""false"" xmlns=""http://github.com/ronin4net/plainion/GraphViz/Packaging/Spec"">
    <Package Name=""System"">
        <Package.Clusters>
            <Cluster Name=""System"">
                <Include Pattern=""*"" />
            </Cluster>
        </Package.Clusters>
        <Include Pattern=""*.dll"" />
    </Package>
</SystemPackaging>
";
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

                // initially specs do not have Ids - generate them once and keep them
                bool idsGenerated = false;
                foreach (var cluster in spec.Packages.SelectMany(p => p.Clusters))
                {
                    if (cluster.Id == null)
                    {
                        cluster.Id = Guid.NewGuid().ToString();
                        idsGenerated = true;
                    }
                }

                if (idsGenerated)
                {
                    // we cannot change the doc while handling a doc change
                    Application.Current.Dispatcher.BeginInvoke(new Action(() =>
                    {
                        Document.Text = SpecUtils.Serialize(spec);
                        Save();
                    }));
                }

                Packages.AddRange(spec.Packages.Select(p => p.Name));
            }
            catch (Exception ex)
            {
                myStatusMessageService.Publish(new StatusMessage("ERROR:" + ex));
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

        public string FileName
        {
            get { return myFileName; }
            set
            {
                SetProperty(ref myFileName, value);
                Document.FileName = myFileName;
            }
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

        public DelegateCommand CreateGraphCommand { get; private set; }

        public DelegateCommand CancelCommand { get; private set; }

        public DelegateCommand OpenCommand { get; private set; }

        public DelegateCommand ClosedCommand { get; private set; }

        public InteractionRequest<OpenFileDialogNotification> OpenFileRequest { get; private set; }

        private void OnOpen()
        {
            var notification = new OpenFileDialogNotification();
            notification.RestoreDirectory = true;
            notification.Filter = "Packaging Spec (*.xaml)|*.xaml|Assembly Graph (*.dot)|*.dot";
            notification.FilterIndex = 0;
            notification.CheckFileExists = false;

            OpenFileRequest.Raise(notification,
                n =>
                {
                    if (!n.Confirmed)
                    {
                        return;
                    }

                    if (File.Exists(n.FileName))
                    {
                        if (Path.GetExtension(n.FileName).Equals(".xaml", StringComparison.OrdinalIgnoreCase))
                        {
                            using (var reader = new StreamReader(n.FileName, true))
                            {
                                Document.Text = reader.ReadToEnd();
                            }
                            FileName = n.FileName;
                        }
                        else
                        {
                            var presentation = myDocumentLoader.Read(n.FileName);
                            var captions = presentation.GetPropertySetFor<Caption>();

                            var spec = new SystemPackaging();

                            foreach (var cluster in presentation.Graph.Clusters)
                            {
                                var caption = captions.Get(cluster.Id).DisplayText;

                                var specCluster = new Spec.Cluster() { Name = caption };
                                specCluster.Patterns.Add(new Include() { Pattern = "*" });

                                var package = new Package() { Name = caption };
                                package.Clusters.Add(specCluster);

                                foreach (var node in cluster.Nodes)
                                {
                                    var assembly = node.Id;
                                    if (!assembly.EndsWith(".dll", StringComparison.OrdinalIgnoreCase))
                                    {
                                        assembly += ".dll";
                                    }

                                    package.Patterns.Add(new Include { Pattern = assembly });
                                }

                                spec.Packages.Add(package);
                            }

                            var specFile = Path.Combine(Path.GetDirectoryName(n.FileName), Path.GetFileNameWithoutExtension(n.FileName) + ".xaml");
                            var text = SpecUtils.Serialize(spec);
                            File.WriteAllText(specFile, text);

                            Document.Text = text;
                            FileName = specFile;
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
                        FileName = n.FileName;
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
                UsedTypesOnly = UsedTypesOnly,
                CreateClustersForNamespaces = CreateClustersForNamespaces
            };

            try
            {
                myCTS = new CancellationTokenSource();

                var doc = await myAnalysisClient.AnalyseAsync(request, myCTS.Token);

                myCTS.Dispose();
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

            var presentation = myPresentationCreationService.CreatePresentation(Path.GetTempPath());

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

            var captionModule = presentation.GetPropertySetFor<Caption>();

            // add potentially empty clusters
            {
                var spec = SpecUtils.Deserialize(Document.Text);
                var emptyClusters = spec.Packages
                    .Where(p => PackagesToAnalyze == null || PackagesToAnalyze.Contains(p.Name))
                    .SelectMany(p => p.Clusters)
                    .Where(cluster => !response.Clusters.Any(c => c.Key == cluster.Id));

                foreach (var cluster in emptyClusters)
                {
                    builder.TryAddCluster(cluster.Id, Enumerable.Empty<string>());
                    captionModule.Add(new Caption(cluster.Id, cluster.Name));
                }
            }

            presentation.Graph = builder.Graph;

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

        private void OnClosed()
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
            if (FileName != null)
            {
                using (var writer = new StreamWriter(FileName, false, Encoding.UTF8))
                {
                    writer.WriteLine(Document.Text);
                }
            }
        }

        protected override void OnPresentationChanged()
        {
            // reset only!
            myGraphToSpecSynchronizer.Presentation = null;
        }

        public bool UsedTypesOnly
        {
            get { return myUsedTypesOnly; }
            set { SetProperty(ref myUsedTypesOnly, value); }
        }

        public bool CreateClustersForNamespaces
        {
            get { return myCreateClustersForNamespaces; }
            set { SetProperty(ref myCreateClustersForNamespaces, value); }
        }
    }
}
