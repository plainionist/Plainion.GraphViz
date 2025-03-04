using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using Plainion.Graphs;
using Plainion.GraphViz.Modules.Metrics.Algorithms;
using Plainion.GraphViz.Presentation;
using Plainion.GraphViz.Viewer.Abstractions.ViewModel;
using Plainion.Prism.Interactivity.InteractionRequest;

namespace Plainion.GraphViz.Modules.Metrics;

class MetricsViewModel : ViewModelBase, IInteractionRequestAware
{
    private Action myFinishAction;
    private CancellationTokenSource myCTS;
    private IReadOnlyCollection<NodeDegreesVM> myDegreeCentrality;
    private GraphDensityVM myGraphDensity;
    private IReadOnlyCollection<CycleVM> myCycles;
    private PathwaysVM myPathways;

    public MetricsViewModel(IDomainModel model)
         : base(model)
    {
        myDegreeCentrality = [];
    }

    public IReadOnlyCollection<NodeDegreesVM> DegreeCentrality
    {
        get { return myDegreeCentrality; }
        set { SetProperty(ref myDegreeCentrality, value); }
    }

    public GraphDensityVM GraphDensity
    {
        get { return myGraphDensity; }
        set { SetProperty(ref myGraphDensity, value); }
    }

    public IReadOnlyCollection<CycleVM> Cycles
    {
        get { return myCycles; }
        set { SetProperty(ref myCycles, value); }
    }

    public PathwaysVM Pathways
    {
        get { return myPathways; }
        set { SetProperty(ref myPathways, value); }
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
        Step(() => { GraphDensity = ComputeGraphDensity(); });
        Step(() => { Cycles = ComputeCycles(); });
        Step(() => { Pathways = ComputePathways(); });
    }

    private IReadOnlyCollection<NodeDegreesVM> ComputeDegreeCentrality()
    {
        var captions = Model.Presentation.GetPropertySetFor<Caption>();

        return Model.Presentation.Graph.Nodes
            .Select(x => new NodeDegreesVM
            {
                Caption = captions.Get(x.Id).DisplayText,
                In = x.In.Count,
                Out = x.Out.Count,
                Total = x.In.Count + x.Out.Count
            })
            .OrderByDescending(x => x.Total)
            .ToList();
    }

    private GraphDensityVM ComputeGraphDensity() =>
        new()
        {
            NodeCount = Model.Presentation.Graph.Nodes.Count,
            EdgeCount = Model.Presentation.Graph.Edges.Count,
            Density = GraphMetricsCalculator.ComputeGraphDensity(Model.Presentation.Graph)
        };

    private IReadOnlyCollection<CycleVM> ComputeCycles()
    {
        var captions = Model.Presentation.GetPropertySetFor<Caption>();

        CycleVM CreateCycleVM(Cycle cycle) =>
            new()
            {
                Start = captions.Get(cycle.Start.Id).DisplayText,
                Path = cycle.Path.Skip(1).Select(n => captions.Get(n.Id).DisplayText).ToList()
            };

        return CycleFinder.FindAllCycles(Model.Presentation.Graph)
            .Select(CreateCycleVM)
            .ToList();
    }

    private PathwaysVM ComputePathways()
    {
        var shortestPaths = ShortestPathsFinder.FindAllShortestPaths(Model.Presentation.Graph);

        var captions = Model.Presentation.GetPropertySetFor<Caption>();

        return new()
        {
            Diameter = GraphMetricsCalculator.ComputeDiameter(shortestPaths),
            AveragePathLength = GraphMetricsCalculator.ComputeAveragePathLength(Model.Presentation.Graph, shortestPaths),
            BetweennessCentrality = GraphMetricsCalculator.ComputeBetweennessCentrality(Model.Presentation.Graph, shortestPaths)
                .Select(x => new KeyValuePair<string, double>(captions.Get(x.Key.Id).DisplayText, x.Value))
                .ToList()
        };
    }

}

