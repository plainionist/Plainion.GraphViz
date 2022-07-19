using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Windows;
using System.Windows.Media;
using Plainion.Collections;
using Plainion.GraphViz.Infrastructure.Services;
using Plainion.GraphViz.Infrastructure.ViewModel;
using Plainion.GraphViz.Model;
using Plainion.GraphViz.Modules.CodeInspection.Core;
using Plainion.GraphViz.Presentation;
using Plainion.Prism.Interactivity.InteractionRequest;
using Prism.Commands;

namespace Plainion.GraphViz.Modules.VsProjects.Dependencies
{
    class ConfigurationViewModel : ViewModelBase
    {
        private readonly IPresentationCreationService myPresentationCreationService;
        private readonly IStatusMessageService myStatusMessageService;
        private readonly Analyzer myAnalyzer;
        private bool myIsReady;
        private CancellationTokenSource myCTS;
        private string myFolderToAnalyze;

        public ConfigurationViewModel(IPresentationCreationService presentationCreationService, IStatusMessageService statusMessageService, Analyzer analyzer, IDomainModel model)
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
            if (myCTS != null)
            {
                myCTS.Cancel();
            }

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

        public bool IgnoreThirdPartyReferences { get; set; }

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
                    OnInheritanceGraphCompleted(doc);
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

            if (myCTS != null)
            {
                myCTS.Cancel();
            }

            IsReady = true;
        }

        private void OnInheritanceGraphCompleted(AnalysisDocument document)
        {
            //if (!document.Edges.Any())
            //{
            //    MessageBox.Show("No nodes found");
            //    return;
            //}

            //var presentation = myPresentationCreationService.CreatePresentation(FolderToAnalyze);

            //var captionModule = presentation.GetPropertySetFor<Caption>();
            //var tooltipModule = presentation.GetPropertySetFor<ToolTipContent>();
            //var edgeStyleModule = presentation.GetPropertySetFor<EdgeStyle>();

            //var builder = new RelaxedGraphBuilder();
            //foreach (var edge in document.Edges)
            //{
            //    var e = builder.TryAddEdge(edge.Item1, edge.Item2);
            //    edgeStyleModule.Add(new EdgeStyle(e.Id)
            //    {
            //        Color = edge.Item3 == ReferenceType.DerivesFrom ? Brushes.Black : Brushes.Blue
            //    });
            //}

            //presentation.Graph = builder.Graph;

            //foreach (var desc in document.Descriptors)
            //{
            //    captionModule.Add(new Caption(desc.Id, desc.Name));
            //    tooltipModule.Add(new ToolTipContent(desc.Id, desc.FullName));
            //}

            //if (document.FailedItems.Any())
            //{
            //    foreach (var item in document.FailedItems)
            //    {
            //        var sb = new StringBuilder();
            //        sb.AppendLine("Loading failed");
            //        sb.AppendFormat("Item: {0}", item.Item);
            //        sb.AppendLine();
            //        sb.AppendFormat("Reason: {0}", item.FailureReason);
            //        myStatusMessageService.Publish(new StatusMessage(sb.ToString()));
            //    }
            //}

            //Model.Presentation = presentation;
        }
    }
}
