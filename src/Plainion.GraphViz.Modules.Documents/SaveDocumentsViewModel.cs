using System.ComponentModel.Composition;
using System.IO;
using System.Linq;
using System.Text;
using Plainion.GraphViz.Infrastructure.Services;
using Plainion.GraphViz.Infrastructure.ViewModel;
using Microsoft.Practices.Prism.Commands;
using Microsoft.Practices.Prism.Interactivity.InteractionRequest;
using Plainion.Prism.Interactivity.InteractionRequest;
using System.ComponentModel;
using Microsoft.Practices.Prism.Mvvm;
using Plainion.GraphViz.Presentation;
using System;
using Plainion.GraphViz.Model;
using Plainion.GraphViz.Dot;

namespace Plainion.GraphViz.Modules.Documents
{
    [Export(typeof(SaveDocumentsViewModel))]
    public class SaveDocumentsViewModel : ViewModelBase
    {
        [Import]
        internal IPresentationCreationService PresentationCreationService { get; set; }

        [Import]
        public IStatusMessageService StatusMessageService { get; set; }

        public SaveDocumentsViewModel()
        {
            SaveDocumentCommand = new DelegateCommand(OnSave, CanSave);
            SaveFileRequest = new InteractionRequest<SaveFileDialogNotification>();
        }

        public DelegateCommand SaveDocumentCommand { get; private set; }

        private bool CanSave()
        {
            return Model.Presentation != null;
        }

        private void OnSave()
        {
            var notification = new SaveFileDialogNotification();
            notification.RestoreDirectory = true;
            notification.Filter = "DOT files (*.dot)|*.dot|DGML files (*.dgml)|*.dgml";
            notification.FilterIndex = 0;
            notification.DefaultExt = ".dot";

            SaveFileRequest.Raise(notification,
                n =>
                {
                    if (n.Confirmed)
                    {
                        Save(n.FileName);
                    }
                });
        }

        public InteractionRequest<SaveFileDialogNotification> SaveFileRequest { get; private set; }

        private void Save(string path)
        {
            if (Path.GetExtension(path).Equals(".dgml", StringComparison.OrdinalIgnoreCase))
            {
                SaveAsDgml(path);
            }
            else
            {
                SaveAsDot(path);
            }
        }

        private void SaveAsDgml(string path)
        {
            var captionModule = Model.Presentation.GetModule<CaptionModule>();
            Func<Node, string> GetNodeCaption = node =>
                {
                    var caption = captionModule.Get(node.Id);
                    if (caption == null)
                    {
                        return node.Id;
                    }
                    return caption.Label != null ? caption.Label : node.Id;
                };

            using (var writer = new StreamWriter(path))
            {
                DgmlExporter.Export(Model.Presentation.Graph, GetNodeCaption, writer);
            }
        }

        private void SaveAsDot(string path)
        {
            var writer = new DotWriter(path);
            writer.Write(Model.Presentation);
        }

        protected override void OnModelPropertyChanged(string propertyName)
        {
            PropertyChangedEventManager.AddHandler(Model, OnPresentationChanged, PropertySupport.ExtractPropertyName(() => Model.Presentation));
        }

        private void OnPresentationChanged(object sender, PropertyChangedEventArgs e)
        {
            SaveDocumentCommand.RaiseCanExecuteChanged();
        }
    }
}
