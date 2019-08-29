using System;
using System.ComponentModel;
using System.ComponentModel.Composition;
using System.IO;
using System.Linq;
using System.Security;
using Plainion.GraphViz.Dot;
using Plainion.GraphViz.Infrastructure.Services;
using Plainion.GraphViz.Infrastructure.ViewModel;
using Plainion.GraphViz.Presentation;
using Plainion.Prism.Interactivity.InteractionRequest;
using Prism.Commands;
using Prism.Interactivity.InteractionRequest;
using Prism.Mvvm;

namespace Plainion.GraphViz.Modules.Documents
{
    [Export(typeof(SaveDocumentsViewModel))]
    public class SaveDocumentsViewModel : ViewModelBase
    {
        [Import]
        public IPresentationCreationService PresentationCreationService { get; set; }

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
            notification.Filter = "DOT files (*.dot)|*.dot|DGML files (*.dgml)|*.dgml|Plainion.GraphViz files (*.pgv)|*.pgv";
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
            else if (Path.GetExtension(path).Equals(".dot", StringComparison.OrdinalIgnoreCase))
            {
                SaveAsDot(path);
            }
            else
            {
                SavePresentation(path);
            }
        }

        private void SaveAsDgml(string path)
        {
            var captionModule = Model.Presentation.GetModule<CaptionModule>();

            var transformationModule = Model.Presentation.GetModule<ITransformationModule>();
            using (var writer = new StreamWriter(path))
            {
                writer.WriteLine("<DirectedGraph xmlns=\"http://schemas.microsoft.com/vs/2009/dgml\">");

                writer.WriteLine("  <Nodes>");

                var visibleNodes = transformationModule.Graph.Nodes
                    .Where(n => Model.Presentation.Picking.Pick(n))
                    .ToList();

                foreach (var node in visibleNodes)
                {
                    var caption = captionModule.Get(node.Id);
                    var label = caption == null ? node.Id : (caption.Label ?? node.Id);

                    writer.WriteLine("    <Node Id=\"{0}\" Label=\"{1}\" />", SecurityElement.Escape(node.Id), SecurityElement.Escape(label));
                }
                writer.WriteLine("  </Nodes>");

                writer.WriteLine("  <Links>");

                var visibleEdges = transformationModule.Graph.Edges
                    .Where(e => visibleNodes.Contains(e.Source) && visibleNodes.Contains(e.Target));

                foreach (var edge in visibleEdges)
                {
                    writer.WriteLine("    <Link Source=\"{0}\" Target=\"{1}\" />", SecurityElement.Escape(edge.Source.Id), SecurityElement.Escape(edge.Target.Id));
                }
                writer.WriteLine("  </Links>");

                writer.WriteLine("</DirectedGraph>");
            }
        }

        private void SaveAsDot(string path)
        {
            var writer = new DotWriter(path);

            if (Model.Presentation.GetModule<IGraphLayoutModule>().Algorithm == LayoutAlgorithm.Flow)
            {
                writer.Settings = DotPresets.Flow;
            }

            writer.Write(Model.Presentation.GetModule<ITransformationModule>().Graph, Model.Presentation.Picking, Model.Presentation);
        }

        private void SavePresentation(string path)
        {
            using (var stream = new FileStream(path, FileMode.Create, FileAccess.Write))
            {
                using (var writer = new BinaryPresentationWriter(stream))
                {
                    writer.Write(Model.Presentation);
                }
            }
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
