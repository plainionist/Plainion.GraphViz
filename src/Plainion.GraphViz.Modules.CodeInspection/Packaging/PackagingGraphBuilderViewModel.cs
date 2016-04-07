using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Windows;
using System.Windows.Markup;
using System.Xml;
using ICSharpCode.AvalonEdit.Document;
using Microsoft.Practices.Prism.Commands;
using Microsoft.Practices.Prism.Interactivity.InteractionRequest;
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
        private string myPackageName;
        private AnalysisMode myAnalysisMode;
        private TextDocument myDocument;
        private IEnumerable<ElementCompletionData> myCompletionData;
        private CancellationTokenSource myCTS;
        private IModuleChangedObserver myTransformationsObserver;

        public PackagingGraphBuilderViewModel()
        {
            Document = new TextDocument();
            Document.Changed += Document_Changed;

            CreateGraphCommand = new DelegateCommand(OnCreateGraph, () => IsReady && IsValidDocument());
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

            AnalysisMode = AnalysisMode.CrossPackageDependencies;
            IsReady = true;
        }

        private bool IsValidDocument()
        {
            if (string.IsNullOrEmpty(Document.Text))
            {
                return false;
            }

            try
            {
                using (var reader = new StringReader(Document.Text))
                {
                    XamlReader.Load(XmlReader.Create(reader));
                }

                return true;
            }
            catch
            {
                return false;
            }
        }

        private void Document_Changed(object sender, DocumentChangeEventArgs e)
        {
            CreateGraphCommand.RaiseCanExecuteChanged();
        }

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

        public AnalysisMode AnalysisMode
        {
            get { return myAnalysisMode; }
            set { SetProperty(ref myAnalysisMode, value); }
        }

        public string PackageName
        {
            get { return myPackageName; }
            set { SetProperty(ref myPackageName, value); }
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
                            Document.Text = File.ReadAllText(n.FileName);
                            try
                            {
                                var spec = SpecUtils.Deserialize(Document.Text);
                                if (spec.Packages.Count == 1)
                                {
                                    AnalysisMode = Packaging.AnalysisMode.InnerPackageDependencies;
                                    PackageName = spec.Packages.Single().Name;
                                }
                            }
                            catch
                            {
                                // we try to optimize usability - ignore exceptions here
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
                AnalysisMode = AnalysisMode,
                PackageName = PackageName,
                OutputFile = Path.GetTempFileName()
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
                captionModule.Add(caption);
            }

            var nodeStyles = presentation.GetPropertySetFor<NodeStyle>();
            foreach (var style in response.NodeStyles)
            {
                nodeStyles.Add(style);
            }

            var edgeStyles = presentation.GetPropertySetFor<EdgeStyle>();
            foreach (var style in response.EdgeStyles)
            {
                edgeStyles.Add(style);
            }

            Model.Presentation = presentation;
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
                File.WriteAllText(Document.FileName, Document.Text);
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
                }

                var transformationModule = Model.Presentation.GetModule<ITransformationModule>();
                myTransformationsObserver = transformationModule.CreateObserver();
                myTransformationsObserver.ModuleChanged += OnTransformationsChanged;
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
                                cluster.Patterns.Add(new Exclude {Pattern = entry.Key});
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
                            clusterToAddTo = new Spec.Cluster {Name = entry.Value};
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
                            clusterToAddTo.Patterns.Add(new Include {Pattern = entry.Key});
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
    }
}
