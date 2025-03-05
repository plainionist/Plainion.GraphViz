using System;
using System.Diagnostics;
using Plainion.GraphViz.Viewer.Abstractions.ViewModel;
using Plainion.Prism.Interactivity.InteractionRequest;
using Prism.Commands;
using Prism.Mvvm;

namespace Plainion.GraphViz.Modules.Metrics;

internal class ToolsMenuItemModel : BindableBase
{
    private readonly IDomainModel myModel;
    private readonly MetricsViewModel myController;

    public ToolsMenuItemModel(IDomainModel model, MetricsViewModel controller)
    {
        myModel = model;
        myController = controller;

        myModel.PresentationChanged += OnPresentationChanged;

        StartAnalysisCommand = new DelegateCommand(OnStartAnalysis);
        StartAnalysisRequest = new InteractionRequest<INotification>();
    }

    private void OnStartAnalysis()
    {
        var notification = new Notification
        {
            Title = "Graph Metrics"
        };

        StartAnalysisRequest.Raise(notification, c => {
            myController.Closed();
        });
    }

    private void OnPresentationChanged(object sender, EventArgs e)
    {
        StartAnalysisCommand.RaiseCanExecuteChanged();
    }

    public DelegateCommand StartAnalysisCommand { get; }
    public InteractionRequest<INotification> StartAnalysisRequest { get; private set; }
}