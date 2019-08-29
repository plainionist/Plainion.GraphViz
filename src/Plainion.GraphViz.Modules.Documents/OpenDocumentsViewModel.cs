using System;
using System.ComponentModel.Composition;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows;
using Plainion.GraphViz.Dot;
using Plainion.GraphViz.Infrastructure.Services;
using Plainion.GraphViz.Infrastructure.ViewModel;
using Plainion.GraphViz.Model;
using Plainion.GraphViz.Presentation;
using Plainion.Prism.Interactivity.InteractionRequest;
using Prism.Commands;
using Prism.Interactivity.InteractionRequest;
using Prism.Mvvm;

namespace Plainion.GraphViz.Modules.Documents
{
    [Export(typeof(IDocumentLoader))]
    [Export(typeof(OpenDocumentsViewModel))]
    class OpenDocumentsViewModel : ViewModelBase, IDocumentLoader
    {
        private FileSystemWatcher myFileWatcher;
        private readonly GraphToDotLangSynchronizer myGraphToDotSynchronizer;
        private IPresentationCreationService myPresentationCreationService;
        private IStatusMessageService myStatusMessageService;

        [ImportingConstructor]
        public OpenDocumentsViewModel(IPresentationCreationService presentationCreationService, IStatusMessageService statusMessageService, IDomainModel model)
            : base(model)
        {
            myPresentationCreationService = presentationCreationService;
            myStatusMessageService = statusMessageService;

            OpenDocumentCommand = new DelegateCommand(OpenDocument);
            OpenFileRequest = new InteractionRequest<OpenFileDialogNotification>();

            myGraphToDotSynchronizer = new GraphToDotLangSynchronizer();
        }

        public DelegateCommand OpenDocumentCommand { get; private set; }

        public InteractionRequest<OpenFileDialogNotification> OpenFileRequest { get; private set; }

        private void OpenDocument()
        {
            var notification = new OpenFileDialogNotification();
            notification.RestoreDirectory = true;
            notification.Filter = "DOT files (*.dot)|*.dot|DGML files (*.dgml)|*.dgml|GraphML files (*.graphml)|*.graphml|DOT plain files (*.plain)|*.plain|Plainion.GraphViz files (*.pgv)|*.pgv";
            notification.FilterIndex = 0;
            notification.DefaultExt = ".dot";

            OpenFileRequest.Raise(notification,
                n =>
                {
                    if (n.Confirmed)
                    {
                        Open(n.FileName);
                    }
                });
        }

        private void Open(string path)
        {
            if (Path.GetExtension(path).Equals(".pgv", StringComparison.OrdinalIgnoreCase))
            {
                OpenPresentation(path);
            }
            else
            {
                OpenDocument(path);
            }
        }

        private void OpenPresentation(string path)
        {
            using (var stream = new FileStream(path, FileMode.Open, FileAccess.Read))
            {
                using (var reader = new BinaryPresentationReader(stream))
                {
                    Model.Presentation = reader.Read();
                }
            }
        }

        private void OpenDocument(string path)
        {
            var presentation = myPresentationCreationService.CreatePresentation(Path.GetDirectoryName(path));

            var processor = new BasicDocumentProcessor(presentation);

            processor.Process(path);

            if (processor.FailedItems.Any())
            {
                var sb = new StringBuilder();
                sb.AppendLine("Following items could not be loaded successfully:");
                sb.AppendLine();
                foreach (var item in processor.FailedItems)
                {
                    sb.AppendLine(string.Format("{0}: {1}", item.FailureReason, item.Item));
                }
                myStatusMessageService.Publish(new StatusMessage(sb.ToString()));
            }

            Model.Presentation = presentation;

            myFileWatcher = new FileSystemWatcher();
            myFileWatcher.Path = Path.GetDirectoryName(path);
            myFileWatcher.Filter = Path.GetFileName(path);
            // http://stackoverflow.com/questions/19905151/system-io-filesystemwatcher-does-not-watch-file-changed-by-visual-studio-2013
            myFileWatcher.NotifyFilter = NotifyFilters.LastWrite | NotifyFilters.CreationTime;
            myFileWatcher.Changed += OnCurrentFileChanged;
            myFileWatcher.Error += OnFileWatcherError;
            myFileWatcher.EnableRaisingEvents = true;

            // only synchronize presentations where we know the doc type and which were created from this module
            if (Path.GetExtension(path).Equals(".dot", StringComparison.OrdinalIgnoreCase))
            {
                myGraphToDotSynchronizer.Attach(presentation, p =>
                    // enqueue to have less blocking of UI
                    Application.Current.Dispatcher.BeginInvoke(new Action(() => SyncToDocument(p, path))));
            }
        }

        private void SyncToDocument(IGraphPresentation p, string path)
        {
            myFileWatcher.EnableRaisingEvents = false;

            using (new Profile("GraphToDotSynchronizer:OnTransformationsChanged"))
            {
                var graph = new Graph();
                foreach (var n in p.Graph.Nodes)
                {
                    graph.TryAdd(n);
                }
                foreach (var e in p.Graph.Edges)
                {
                    graph.TryAdd(e);
                }

                var transformationModule = p.GetModule<ITransformationModule>();
                foreach (var cluster in transformationModule.Graph.Clusters.OrderBy(c => c.Id))
                {
                    // we do not want to see the pseudo node added for folding but the full expanded list of nodes of this cluster
                    var folding = transformationModule.Items
                        .OfType<ClusterFoldingTransformation>()
                        .SingleOrDefault(f => f.Clusters.Contains(cluster.Id));

                    // the nodes we get through ITransformationModule might be new instances!
                    // -> get the right node instances based on the returned ids
                    //    (otherwise the writer below will remove them from the output)
                    var nodes = (folding == null ? cluster.Nodes : folding.GetNodes(cluster.Id))
                        .Select(n => graph.GetNode(n.Id))
                        .ToList();

                    graph.TryAdd(new Cluster(cluster.Id, nodes));
                }

                var writer = new DotWriter(path);
                writer.PrettyPrint = true;
                if (p.GetModule<IGraphLayoutModule>().Algorithm == LayoutAlgorithm.Flow)
                {
                    writer.Settings = DotPresets.Flow;
                }

                writer.Write(graph, new NullGraphPicking(), p);
            }

            myFileWatcher.EnableRaisingEvents = true;
        }

        private void OnCurrentFileChanged(object sender, FileSystemEventArgs e)
        {
            Application.Current.Dispatcher.BeginInvoke(new Action(() => Open(e.FullPath)));
        }

        private void OnFileWatcherError(object sender, ErrorEventArgs e)
        {
            Debug.WriteLine(e.GetException().Dump());
        }

        protected override void OnPresentationChanged()
        {
            if (myFileWatcher != null)
            {
                myFileWatcher.Dispose();
                myFileWatcher = null;
            }

            // reset only!
            myGraphToDotSynchronizer.Detach();
        }

        public bool CanLoad(string filename)
        {
            var ext = Path.GetExtension(filename);
            return ext.Equals(".dot", StringComparison.OrdinalIgnoreCase)
                || ext.Equals(".dgml", StringComparison.OrdinalIgnoreCase)
                || ext.Equals(".graphml", StringComparison.OrdinalIgnoreCase)
                || ext.Equals(".plain", StringComparison.OrdinalIgnoreCase)
                || ext.Equals(".pgv", StringComparison.OrdinalIgnoreCase);
        }

        public void Load(string filename)
        {
            Open(filename);
        }

        public IGraphPresentation Read(string filename)
        {
            var presentation = myPresentationCreationService.CreatePresentation(Path.GetDirectoryName(filename));

            var processor = new BasicDocumentProcessor(presentation);

            processor.Process(filename);

            return presentation;
        }
    }
}
