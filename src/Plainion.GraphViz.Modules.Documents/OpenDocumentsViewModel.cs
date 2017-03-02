using System;
using System.ComponentModel.Composition;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows;
using Microsoft.Practices.Prism.Commands;
using Microsoft.Practices.Prism.Interactivity.InteractionRequest;
using Microsoft.Practices.Prism.Mvvm;
using Plainion.GraphViz.Infrastructure.Services;
using Plainion.GraphViz.Infrastructure.ViewModel;
using Plainion.Prism.Interactivity.InteractionRequest;
using Plainion.GraphViz.Dot;
using System.Threading.Tasks;

namespace Plainion.GraphViz.Modules.Documents
{
    [Export(typeof(IDocumentLoader))]
    [Export(typeof(OpenDocumentsViewModel))]
    public class OpenDocumentsViewModel : ViewModelBase, IDocumentLoader
    {
        private FileSystemWatcher myFileWatcher;
        private readonly GraphToDotLangSynchronizer myGraphToDotSynchronizer;

        [Import]
        internal IPresentationCreationService PresentationCreationService { get; set; }

        [Import]
        public IStatusMessageService StatusMessageService { get; set; }

        public OpenDocumentsViewModel()
        {
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
            notification.Filter = "DOT files (*.dot)|*.dot|DGML files (*.dgml)|*.dgml|GraphML files (*.graphml)|*.graphml|DOT plain files (*.plain)|*.plain";
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
            var presentation = PresentationCreationService.CreatePresentation(Path.GetDirectoryName(path));

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
                StatusMessageService.Publish(new StatusMessage(sb.ToString()));
            }

            Model.Presentation = presentation;

            if (myFileWatcher != null)
            {
                myFileWatcher.Dispose();
                myFileWatcher = null;
            }

            myFileWatcher = new FileSystemWatcher();
            myFileWatcher.Path = Path.GetDirectoryName(path);
            myFileWatcher.Filter = Path.GetFileName(path);
            myFileWatcher.NotifyFilter = NotifyFilters.LastWrite;
            myFileWatcher.Changed += OnCurrentFileChanged;
            myFileWatcher.EnableRaisingEvents = true;

            // only synchronize presentations where we know the doc type and which were created from this module
            if (Path.GetExtension(path).Equals(".dot", StringComparison.OrdinalIgnoreCase))
            {
                myGraphToDotSynchronizer.Attach(presentation, p => Task.Run(() =>
                {

                    myFileWatcher.EnableRaisingEvents = false;

                    var writer = new DotWriter(path);
                    writer.Write(p);

                    myFileWatcher.EnableRaisingEvents = true;
                }));
            }
        }

        private void OnCurrentFileChanged(object sender, FileSystemEventArgs e)
        {
            Application.Current.Dispatcher.BeginInvoke(new Action(() => Open(e.FullPath)));
        }

        protected override void OnModelPropertyChanged(string propertyName)
        {
            if (propertyName == PropertySupport.ExtractPropertyName(() => Model.Presentation))
            {
                if (myFileWatcher != null)
                {
                    myFileWatcher.Dispose();
                    myFileWatcher = null;
                }

                // reset only!
                myGraphToDotSynchronizer.Detach();
            }
        }

        public bool CanLoad(string filename)
        {
            var ext = Path.GetExtension(filename);
            return ext.Equals(".dot", StringComparison.OrdinalIgnoreCase)
                || ext.Equals(".dgml", StringComparison.OrdinalIgnoreCase)
                || ext.Equals(".graphml", StringComparison.OrdinalIgnoreCase)
                || ext.Equals(".plain", StringComparison.OrdinalIgnoreCase);
        }

        public void Load(string filename)
        {
            Open(filename);
        }
    }
}
