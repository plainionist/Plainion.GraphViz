using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using Plainion.GraphViz.Viewer.Abstractions.ViewModel;
using Plainion.Prism.Interactivity.InteractionRequest;

namespace Plainion.GraphViz.Modules.Metrics;

class MetricsViewModel : ViewModelBase, IInteractionRequestAware
{
    private Action myFinishAction;
    private CancellationTokenSource myCTS;

    public MetricsViewModel(IDomainModel model)
         : base(model)
    {
    }

    public ObservableCollection<KeyValuePair<string, string>> Results { get; } = [];

    protected override void OnPresentationChanged()
    {
        myCTS?.Cancel();
    }

    public INotification Notification { get; set; }

    public Action FinishInteraction
    {
        get { return myFinishAction; }
        set
        {
            if (SetProperty(ref myFinishAction, value))
            {
                TriggerAnalysis();
            }
        }
    }

    private void TriggerAnalysis()
    {
        if (Results.Count != 0)
        {
            // results already available
            return;
        }

        if (Model.Presentation == null)
        {
            // no data to analyze
            return;
        }

        if (myCTS != null)
        {
            // analysis already running
            return;
        }

        myCTS = new CancellationTokenSource();


        Task.Run(async () => await RunAnalysis(myCTS.Token), myCTS.Token);
    }

    private async Task RunAnalysis(CancellationToken token)
    {
        Debug.WriteLine("running analysis");

        await Task.Delay(2000);

        Application.Current.Dispatcher.Invoke(() =>
        {
            Results.Add(new KeyValuePair<string, string>("1", "a"));
        });

        await Task.Delay(2000);

        Application.Current.Dispatcher.Invoke(() =>
        {
            Results.Add(new KeyValuePair<string, string>("2", "b"));
        });

        await Task.Delay(2000);

        Application.Current.Dispatcher.Invoke(() =>
        {
            Results.Add(new KeyValuePair<string, string>("3", "c"));
        });
    }
}

