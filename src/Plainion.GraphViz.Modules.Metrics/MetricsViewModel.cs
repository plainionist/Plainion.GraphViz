using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using Plainion.Graphs;
using Plainion.GraphViz.Presentation;
using Plainion.GraphViz.Viewer.Abstractions.ViewModel;
using Plainion.Prism.Interactivity.InteractionRequest;

namespace Plainion.GraphViz.Modules.Metrics;

class MetricsViewModel : ViewModelBase, IInteractionRequestAware
{
    private Action myFinishAction;
    private CancellationTokenSource myCTS;
    private IReadOnlyCollection<NodeDegrees> myDegreeCentrality;
    private GraphDensity myGraphDensity;
    private IReadOnlyCollection<GraphCycle> myCycles;

    public MetricsViewModel(IDomainModel model)
         : base(model)
    {
        myDegreeCentrality = [];
    }

    public IReadOnlyCollection<NodeDegrees> DegreeCentrality
    {
        get { return myDegreeCentrality; }
        set { SetProperty(ref myDegreeCentrality, value); }
    }

    public GraphDensity GraphDensity
    {
        get { return myGraphDensity; }
        set { SetProperty(ref myGraphDensity, value); }
    }

    public IReadOnlyCollection<GraphCycle> Cycles
    {
        get { return myCycles; }
        set { SetProperty(ref myCycles, value); }
    }

    protected override void OnPresentationChanged()
    {
        myCTS?.Cancel();

        myDegreeCentrality = [];
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
        if (myDegreeCentrality.Count != 0)
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
        void Step(Action action)
        {
            Application.Current.Dispatcher.BeginInvoke(action);
            token.ThrowIfCancellationRequested();
        }

        Step(() => { DegreeCentrality = ComputeDegreeCentrality(); });
        Step(() => { GraphDensity = GraphMetricsCalculator.ComputeGraphDensity(Model.Presentation.Graph); });
        Step(() => { Cycles = ComputeCycles(); });
    }

    private IReadOnlyCollection<NodeDegrees> ComputeDegreeCentrality()
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

    private IReadOnlyCollection<GraphCycle> ComputeCycles()
    {
        var captions = Model.Presentation.GetPropertySetFor<Caption>();

        GraphCycle CreateCycle(IReadOnlyCollection<Node> nodes) =>
            new()
            {
                Start = captions.Get(nodes.First().Id).DisplayText,
                Path = nodes.Skip(1).Select(n => captions.Get(n.Id).DisplayText).ToList()
            };

        return CycleFinder.FindAllCycles(Model.Presentation.Graph)
            .Select(CreateCycle)
            .ToList();
    }
}

