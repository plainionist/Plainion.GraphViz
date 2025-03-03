using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;
using Plainion.GraphViz.Presentation;
using Plainion.GraphViz.Viewer.Abstractions.ViewModel;
using Plainion.Prism.Interactivity.InteractionRequest;

namespace Plainion.GraphViz.Modules.Metrics;

class MetricsViewModel : ViewModelBase, IInteractionRequestAware
{
    private Action myFinishAction;
    private CancellationTokenSource myCTS;
    private IReadOnlyCollection<NodeDegrees> myNodeDegrees;

    public MetricsViewModel(IDomainModel model)
         : base(model)
    {
        myNodeDegrees = [];
    }

    public IReadOnlyCollection<NodeDegrees> NodeDegrees
    {
        get { return myNodeDegrees; }
        set { SetProperty(ref myNodeDegrees, value); }
    }

    protected override void OnPresentationChanged()
    {
        myCTS?.Cancel();

        myNodeDegrees = [];
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
        if (myNodeDegrees.Count != 0)
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

        Task.Run(() => RunAnalysis(myCTS.Token), myCTS.Token)
            .ContinueWith(_ => { myCTS = null; });

    }

    private void RunAnalysis(CancellationToken token)
    {
        var report = ComputeDegrees();
        Application.Current.Dispatcher.BeginInvoke(() => { NodeDegrees = report; });
        token.ThrowIfCancellationRequested();
    }

    private IReadOnlyCollection<NodeDegrees> ComputeDegrees()
    {
        var captions = Model.Presentation.GetPropertySetFor<Caption>();

        return Model.Presentation.Graph.Nodes
            .Select(x => new NodeDegrees
            {
                Caption = captions.Get(x.Id).DisplayText,
                In = x.In.Count,
                Out = x.Out.Count,
                Total = x.In.Count + x.Out.Count
            })
            .OrderByDescending(x => x.Total)
            .ToList();
    }
}

