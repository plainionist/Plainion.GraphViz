using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows;
using Plainion.Graphs;
using Plainion.Graphs.Projections;
using Plainion.GraphViz.Dot;
using Plainion.GraphViz.Presentation;
using Plainion.GraphViz.Viewer.Abstractions.Services;
using Plainion.GraphViz.Viewer.Abstractions.ViewModel;
using Plainion.Prism.Interactivity.InteractionRequest;
using Prism.Commands;
using Path = System.IO.Path;

namespace Plainion.GraphViz.Modules.Documents
{
    class OpenDocumentsViewModel : ViewModelBase, IDocumentLoader
    {
        private FileSystemWatcher myFileWatcher;
        private readonly GraphToDotLangSynchronizer myGraphToDotSynchronizer;
        private readonly IPresentationCreationService myPresentationCreationService;
        private readonly IStatusMessageService myStatusMessageService;
        private DateTime myLastWrittenByApp;

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
            myStatusMessageService.Publish(new StatusMessage($"Opening '{path}'"));

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

            DestroyFileWatcher();
            CreateFileWatcher(path);

            // only synchronize presentations where we know the doc type and which were created from this module
            if (Path.GetExtension(path).Equals(".dot", StringComparison.OrdinalIgnoreCase))
            {
                myGraphToDotSynchronizer.Attach(presentation, p =>
                    // enqueue to have less blocking of UI
                    Application.Current.Dispatcher.BeginInvoke(new Action(() => SyncToDocument(p, path))));
            }
        }

        private void CreateFileWatcher(string path)
        {
            myFileWatcher = new FileSystemWatcher();
            myFileWatcher.Path = Path.GetDirectoryName(path);
            myFileWatcher.Filter = Path.GetFileName(path);
            // http://stackoverflow.com/questions/19905151/system-io-filesystemwatcher-does-not-watch-file-changed-by-visual-studio-2013
            myFileWatcher.NotifyFilter = NotifyFilters.LastWrite | NotifyFilters.CreationTime;
            myFileWatcher.Changed += OnCurrentFileChanged;
            myFileWatcher.Error += OnFileWatcherError;
            myFileWatcher.EnableRaisingEvents = true;
        }

        private void DestroyFileWatcher()
        {
            if (myFileWatcher == null)
            {
                return;
            }

            myFileWatcher.Changed -= OnCurrentFileChanged;
            myFileWatcher.Error -= OnFileWatcherError;

            myFileWatcher.Dispose();
            myFileWatcher = null;
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
                var dynamicClusters = transformationModule.Items.OfType<DynamicClusterTransformation>().SingleOrDefault();
                foreach (var cluster in transformationModule.Graph.Clusters.OrderBy(c => c.Id))
                {
                    // when cluster was deleted, it is invisible
                    var isHidden = dynamicClusters != null && dynamicClusters.ClusterVisibility.TryGetValue(cluster.Id, out var isVisible) && !isVisible;
                    if (isHidden)
                    {
                        continue;
                    }

                    // we do not want to see the pseudo node added for folding but the full expanded list of nodes of this cluster
                    var folding = transformationModule.Items.OfType<ClusterFoldingTransformation>()
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

                writer.Write(graph, new NullGraphPicking(), p);
                myLastWrittenByApp = File.GetLastWriteTimeUtc(path);
            }

            myFileWatcher.EnableRaisingEvents = true;
        }

        private void OnCurrentFileChanged(object sender, FileSystemEventArgs e)
        {
            var lastWriteTime = File.GetLastWriteTimeUtc(e.FullPath);

            if ((lastWriteTime - myLastWrittenByApp).TotalMilliseconds < 100)
            {
                // there seems to be a race-condition within file watcher which causes the handler being called 
                // when we write the file from this app even thought we have disabled event sending
                return;
            }

            Application.Current.Dispatcher.BeginInvoke(new Action(() => HotReOpen(e.FullPath, 3)));
        }

        // hot reloading the document while it is edited with notpad works but 
        // when trying the same with VS Code we get "file is use by another process"
        // so lets apply some tolerance
        private void HotReOpen(string file, int remainingRetries)
        {
            try
            {
                Open(file);
            }
            catch
            {
                if (remainingRetries > 0)
                {
                    Application.Current.Dispatcher.BeginInvoke(new Action(() => HotReOpen(file, --remainingRetries)));
                }
            }
        }

        private void OnFileWatcherError(object sender, ErrorEventArgs e)
        {
            Debug.WriteLine(e.GetException().Dump());
        }

        protected override void OnPresentationChanged()
        {
            DestroyFileWatcher();

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
