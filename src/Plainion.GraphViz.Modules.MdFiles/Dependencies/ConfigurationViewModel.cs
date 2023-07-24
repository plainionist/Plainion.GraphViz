using System.Linq;
using System.Threading;
using System.Windows;
using System.Windows.Media;
using Plainion.GraphViz.Infrastructure.Services;
using Plainion.GraphViz.Infrastructure.ViewModel;
using Plainion.GraphViz.Model;
using Plainion.GraphViz.Modules.MdFiles.Dependencies.Analyzer;
using Plainion.GraphViz.Presentation;
using Plainion.Prism.Interactivity.InteractionRequest;
using Prism.Commands;

namespace Plainion.GraphViz.Modules.MdFiles.Dependencies
{
    internal class ConfigurationViewModel : ViewModelBase
    {
        private readonly IPresentationCreationService myPresentationCreationService;
        private readonly IStatusMessageService myStatusMessageService;
        private readonly MarkdownAnalyzer myAnalyzer;
        private bool myIsReady;
        private CancellationTokenSource myCTS;
        private string myFolderToAnalyze;

        public ConfigurationViewModel(IPresentationCreationService presentationCreationService, IStatusMessageService statusMessageService, MarkdownAnalyzer analyzer, IDomainModel model)
            : base(model)
        {
            myPresentationCreationService = presentationCreationService;
            myStatusMessageService = statusMessageService;
            myAnalyzer = analyzer;

            CreateGraphCommand = new DelegateCommand(CreateGraph, () => FolderToAnalyze != null && IsReady);
            CancelCommand = new DelegateCommand(OnCancel, () => !IsReady);
            BrowseFolderCommand = new DelegateCommand(OnBrowseClicked, () => IsReady);
            ClosedCommand = new DelegateCommand(OnClosed);

            OpenFolderRequest = new InteractionRequest<SelectFolderDialogNotification>();

            IsReady = true;
        }

        private void OnCancel()
        {
            myCTS?.Cancel();

            IsReady = true;
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
                    BrowseFolderCommand.RaiseCanExecuteChanged();
                }
            }
        }

        public string FolderToAnalyze
        {
            get { return myFolderToAnalyze; }
            set { SetProperty(ref myFolderToAnalyze, value); }
        }

        public bool ShowInvalidReferences { get; set; }

        public bool ShowExternalReferences { get; set; }

        public DelegateCommand CreateGraphCommand { get; private set; }

        public DelegateCommand CancelCommand { get; private set; }

        public DelegateCommand BrowseFolderCommand { get; private set; }

        public DelegateCommand ClosedCommand { get; private set; }

        public InteractionRequest<SelectFolderDialogNotification> OpenFolderRequest { get; private set; }

        private void OnBrowseClicked()
        {
            var notification = new SelectFolderDialogNotification();

            OpenFolderRequest.Raise(notification,
                n =>
                {
                    if (n.Confirmed)
                    {
                        FolderToAnalyze = n.SelectedPath;
                    }

                    CreateGraphCommand.RaiseCanExecuteChanged();
                });
        }

        private async void CreateGraph()
        {
            IsReady = false;

            try
            {
                myCTS = new CancellationTokenSource();

                var doc = await myAnalyzer.AnalyzeAsync(FolderToAnalyze, myCTS.Token);

                myCTS.Dispose();
                myCTS = null;

                if (doc != null)
                {
                    OnAnalysisCompleted(doc);
                }
            }
            finally
            {
                IsReady = true;
            }
        }

        private void OnClosed()
        {
            FolderToAnalyze = null;

            myCTS?.Cancel();

            IsReady = true;
        }

        private void OnAnalysisCompleted(AnalysisDocument document)
        {
            if (!document.Files.Any())
            {
                MessageBox.Show("No markdown files found");
                return;
            }

            var presentation = myPresentationCreationService.CreatePresentation(FolderToAnalyze);

            var captionModule = presentation.GetPropertySetFor<Caption>();
            var tooltipModule = presentation.GetPropertySetFor<ToolTipContent>();
            var edgeStyleModule = presentation.GetPropertySetFor<EdgeStyle>();

            var builder = new RelaxedGraphBuilder();

            foreach (var file in document.Files)
            {
                builder.TryAddNode(file.FullPath);

                captionModule.Add(new Caption(file.FullPath, file.Name));
                tooltipModule.Add(new ToolTipContent(file.FullPath, file.FullPath));

                foreach (var reference in file.ValidInternalMDRefs)
                {
                    var e = builder.TryAddEdge(file.FullPath, reference);
                    edgeStyleModule.Add(new EdgeStyle(e.Id)
                    {
                        Color = Brushes.Blue
                    });
                }

                if (ShowExternalReferences)
                {
                    foreach (var reference in file.ValidExternalMDRefs)
                    {
                        var e = builder.TryAddEdge(file.FullPath, reference);
                        edgeStyleModule.Add(new EdgeStyle(e.Id)
                        {
                            Color = Brushes.Green
                        });
                    }
                }

                if (ShowInvalidReferences)
                {
                    foreach (var reference in file.InvalidInternalMDRefs)
                    {
                        var e = builder.TryAddEdge(file.FullPath, reference);
                        edgeStyleModule.Add(new EdgeStyle(e.Id)
                        {
                            Color = Brushes.Red
                        });
                    }
                }

                if (ShowInvalidReferences && ShowExternalReferences)
                {
                    foreach (var reference in file.InvalidExternalMDRefs)
                    {
                        var e = builder.TryAddEdge(file.FullPath, reference);
                        edgeStyleModule.Add(new EdgeStyle(e.Id)
                        {
                            Color = Brushes.Red
                        });
                    }
                }
            }

            presentation.Graph = builder.Graph;

            foreach (var item in document.FailedItems)
            {
                var msg = new StatusMessage($"Failed to analyze '{item.FullPath}' with {item.Exception}");
                myStatusMessageService.Publish(msg);
            }

            Model.Presentation = presentation;
        }
    }
}