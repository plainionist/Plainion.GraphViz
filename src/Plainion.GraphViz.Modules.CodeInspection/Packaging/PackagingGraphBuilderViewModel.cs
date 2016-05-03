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
using System.Xml;
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
        private IModuleChangedObserver myTransformationsObserver;
        private bool myUsedTypesOnly;
        private bool myCreateClustersForNamespaces;

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
            catch
            {
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

            // only register for our own presentation
            var transformationModule = Model.Presentation.GetModule<ITransformationModule>();
            myTransformationsObserver = transformationModule.CreateObserver();
            myTransformationsObserver.ModuleChanged += OnTransformationsChanged;
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
            if (propertyName == "Presentation" && Model.Presentation != null)
            {
                if (myTransformationsObserver != null)
                {
                    myTransformationsObserver.ModuleChanged -= OnTransformationsChanged;
                    myTransformationsObserver.Dispose();
                    myTransformationsObserver = null;
                }
            }
        }

        private void OnTransformationsChanged(object sender, EventArgs eventArgs)
        {
            using (new Profile("PackagingGraphBuilderViewModel:OnTransformationsChanged"))
            {
                var spec = SpecUtils.Deserialize(Document.Text);

                var clusters = spec.Packages
                    .SelectMany(p => p.Clusters)
                    .ToList();

                var transformationModule = Model.Presentation.GetModule<ITransformationModule>();
                foreach (var transformation in transformationModule.Items.OfType<DynamicClusterTransformation>())
                {
                    foreach (var entry in transformation.NodeToClusterMapping)
                    {
                        var clustersMatchingNode = clusters
                            .Where(c => c.Matches(entry.Key))
                            .ToList();

                        // remove from all (potentially old) clusters
                        var clustersToRemoveFrom = clustersMatchingNode
                            .Where(c => entry.Value == null || c.Name != entry.Value);
                        foreach (var cluster in clustersToRemoveFrom)
                        {
                            var exactMatch = cluster.Includes.FirstOrDefault(p => p.Pattern == entry.Key);
                            if (exactMatch != null)
                            {
                                cluster.Patterns.Remove(exactMatch);
                            }
                            else
                            {
                                cluster.Patterns.Add(new Exclude { Pattern = entry.Key });
                            }
                        }

                        if (entry.Value == null)
                        {
                            continue;
                        }

                        // add to the cluster it should now belong to
                        var clusterToAddTo = clusters
                            .FirstOrDefault(c => c.Name == entry.Value);

                        if (clusterToAddTo == null)
                        {
                            // --> new cluster added in UI
                            clusterToAddTo = new Spec.Cluster { Name = entry.Value };
                            clusters.Add(clusterToAddTo);
                            spec.Packages.First().Clusters.Add(clusterToAddTo);
                        }

                        if (clusterToAddTo.Matches(entry.Key))
                        {
                            // node already or again matched
                            // -> ignore
                            continue;
                        }
                        else
                        {
                            clusterToAddTo.Patterns.Add(new Include { Pattern = entry.Key });
                        }
                    }

                    foreach (var removedCluster in transformation.ClusterVisibility.Where(e => e.Value == false))
                    {
                        foreach (var package in spec.Packages)
                        {
                            var cluster = package.Clusters.FirstOrDefault(c => c.Name == removedCluster.Key);
                            if (cluster != null)
                            {
                                package.Clusters.Remove(cluster);
                                break;
                            }
                        }

                    }
                }

                Document.Text = SpecUtils.Serialize(spec);

                Save();
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

        public bool CreateClustersForNamespaces
        {
            get { return myCreateClustersForNamespaces; }
            set { SetProperty(ref myCreateClustersForNamespaces, value); }
        }
    }
}
