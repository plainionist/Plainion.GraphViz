using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Windows;
using System.Windows.Media;
using Plainion.GraphViz.Viewer.Abstractions.Services;
using Plainion.GraphViz.Viewer.Abstractions.ViewModel;
using Plainion.Graphs;
using Plainion.GraphViz.Presentation;
using Plainion.Prism.Interactivity.InteractionRequest;
using Prism.Commands;

namespace Plainion.GraphViz.Modules.VsProjects
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

            IgnoreThirdPartyReferences = true;
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

            if (myCTS != null)
            {
                myCTS.Cancel();
            }

            IsReady = true;
        }

        private void OnAnalysisCompleted(AnalysisDocument document)
        {
            if (!document.Projects.Any())
            {
                MessageBox.Show("No projects found");
                return;
            }

            var presentation = myPresentationCreationService.CreatePresentation(FolderToAnalyze);

            var captionModule = presentation.GetPropertySetFor<Caption>();
            var tooltipModule = presentation.GetPropertySetFor<ToolTipContent>();
            var edgeStyleModule = presentation.GetPropertySetFor<EdgeStyle>();

            var projectsByAssembly = document.Projects
                .ToDictionary(x => x.Assembly, StringComparer.OrdinalIgnoreCase);

            var builder = new RelaxedGraphBuilder();
            foreach (var project in document.Projects)
            {
                captionModule.Add(new Caption(project.RelativePath, project.Name));
                tooltipModule.Add(new ToolTipContent(project.RelativePath, project.RelativePath));

                // ProjectReferences cannot be third party as those are part of the
                // analyzed code base
                foreach (var reference in project.ProjectReferences)
                {
                    var e = builder.TryAddEdge(project.RelativePath, reference);
                    edgeStyleModule.Add(new EdgeStyle(e.Id)
                    {
                        Color = Brushes.Blue
                    });
                }

                // Assembly references might belong to the code base or might
                // be third party 
                foreach (var reference in project.References)
                {
                    projectsByAssembly.TryGetValue(reference, out var referencedProject);

                    if (IgnoreThirdPartyReferences && referencedProject == null)
                    {
                        continue;
                    }

                    var targetId = referencedProject != null ? referencedProject.RelativePath : reference;
                    var e = builder.TryAddEdge(project.RelativePath, targetId);
                    edgeStyleModule.Add(new EdgeStyle(e.Id)
                    {
                        Color = Brushes.Green
                    });
                }

                // PackageReferences are considered to be third party always
                if (!IgnoreThirdPartyReferences)
                {
                    foreach (var reference in project.PackageReferences)
                    {
                        var e = builder.TryAddEdge(project.RelativePath, reference);
                        edgeStyleModule.Add(new EdgeStyle(e.Id)
                        {
                            Color = Brushes.Brown
                        });
                    }
                }
            }

            var projectsByTopLevelFolder = document.Projects
                .Select(x => (tokens: x.RelativePath.Split('/', '\\'), project: x))
                .Where(x => x.tokens.Length > 1)
                .GroupBy(x => x.tokens.First(), x => x.project);

            foreach (var cluster in projectsByTopLevelFolder)
            {
                builder.TryAddCluster(cluster.Key, cluster.Select(x => x.RelativePath));
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
