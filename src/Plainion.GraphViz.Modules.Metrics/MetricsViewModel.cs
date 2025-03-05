using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using Plainion.Graphs;
using Plainion.GraphViz.Modules.Metrics.Algorithms;
using Plainion.GraphViz.Presentation;
using Plainion.GraphViz.Viewer.Abstractions;
using Plainion.GraphViz.Viewer.Abstractions.ViewModel;
using Plainion.Prism.Interactivity.InteractionRequest;
using Plainion.Windows.Mvvm;
using Prism.Events;

namespace Plainion.GraphViz.Modules.Metrics;

class MetricsViewModel : ViewModelBase, IInteractionRequestAware
{
    private readonly IEventAggregator myEventAggregator;
    private Action myFinishAction;
    private CancellationTokenSource myCTS;
    private IReadOnlyCollection<NodeDegreesVM> myDegreeCentrality;
    private GraphDensityVM myGraphDensity;
    private IReadOnlyCollection<CycleVM> myCycles;
    private PathwaysVM myPathways;
    private IModuleChangedObserver myNodeMaskObserver;
    private IModuleChangedObserver myTransformationsObserver;

    public MetricsViewModel(IDomainModel model, IEventAggregator eventAggregator)
         : base(model)
    {
        myEventAggregator = eventAggregator;

        myDegreeCentrality = [];

        HighlightCommand = new DelegateCommand<object>(OnHighlight);
    }

    private void OnHighlight(object item)
    {
        if (item is NodeDegreesVM nodeDegreesVM)
        {
            var selection = Model.Presentation.GetPropertySetFor<Selection>();
            selection.Clear();
            selection.Get(nodeDegreesVM.Model.Id).IsSelected = true;

            myEventAggregator.GetEvent<NodeFocusedEvent>().Publish(nodeDegreesVM.Model);
        }
        else if (item is CycleVM cycleVM)
        {
            var selection = Model.Presentation.GetPropertySetFor<Selection>();
            selection.Clear();
            foreach (var pathItem in cycleVM.Model.Path)
            {
                selection.Get(pathItem.Id).IsSelected = true;
            }

            myEventAggregator.GetEvent<NodeFocusedEvent>().Publish(cycleVM.Model.Start);
        }
        else if (item is GraphItemMeasurementVM itemMeasurementVM)
        {
            var selection = Model.Presentation.GetPropertySetFor<Selection>();
            selection.Clear();

            if (itemMeasurementVM.Model is Node node)
            {
                selection.Get(node.Id).IsSelected = true;
                myEventAggregator.GetEvent<NodeFocusedEvent>().Publish(node);
            }
            else if (itemMeasurementVM.Model is Edge edge)
            {
                selection.Get(edge.Id).IsSelected = true;
                selection.Get(edge.Source.Id).IsSelected = true;
                selection.Get(edge.Target.Id).IsSelected = true;

                myEventAggregator.GetEvent<NodeFocusedEvent>().Publish(edge.Source);
            }
            else
            {
                // intentionally ignore
            }
        }
    }

    public DelegateCommand<object> HighlightCommand { get; set; }

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
        if (myNodeMaskObserver != null)
        {
            myNodeMaskObserver.ModuleChanged -= OnNodeMasksChanged;
            myNodeMaskObserver.Dispose();
            myNodeMaskObserver = null;
        }

        if (myTransformationsObserver != null)
        {
            myTransformationsObserver.ModuleChanged -= OnTransformationsChanged;
            myTransformationsObserver.Dispose();
            myTransformationsObserver = null;
        }

        if (Model.Presentation == null)
        {
            return;
        }

        myNodeMaskObserver = Model.Presentation.GetModule<INodeMaskModule>().CreateObserver();
        myNodeMaskObserver.ModuleChanged += OnNodeMasksChanged;

        myTransformationsObserver = Model.Presentation.GetModule<ITransformationModule>().CreateObserver();
        myTransformationsObserver.ModuleChanged += OnTransformationsChanged;

        RestartAnalysis();
    }

    private void RestartAnalysis()
    {
        myDegreeCentrality = [];
        myCTS?.Cancel();
        TriggerAnalysis();
    }

    private void OnTransformationsChanged(object sender, EventArgs e)
    {
        RestartAnalysis();
    }

    private void OnNodeMasksChanged(object sender, EventArgs e)
    {
        RestartAnalysis();
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

        if (myFinishAction == null)
        {
            // dialog not open
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

        var builder = new RelaxedGraphBuilder();
        foreach (var node in Model.Presentation.TransformedGraph.Nodes.Where(Model.Presentation.Picking.Pick))
        {
            builder.TryAddNode(node.Id);
        }
        foreach (var edge in Model.Presentation.TransformedGraph.Edges.Where(Model.Presentation.Picking.Pick))
        {
            builder.TryAddEdge(edge.Source.Id, edge.Target.Id);
        }
        foreach (var cluster in Model.Presentation.TransformedGraph.Clusters.Where(Model.Presentation.Picking.Pick))
        {
            builder.TryAddCluster(cluster.Id, cluster.Nodes.Where(Model.Presentation.Picking.Pick).Select(x => x.Id));
        }

        Step(() => { DegreeCentrality = ComputeDegreeCentrality(Model.Presentation, builder.Graph); });
        Step(() => { GraphDensity = ComputeGraphDensity(Model.Presentation, builder.Graph); });
        Step(() => { Cycles = ComputeCycles(Model.Presentation, builder.Graph); });
        Step(() => { Pathways = ComputePathways(Model.Presentation, builder.Graph); });
    }

    private static IReadOnlyCollection<NodeDegreesVM> ComputeDegreeCentrality(IModuleRepository modules, IGraph graph)
    {
        var captions = modules.GetPropertySetFor<Caption>();

        return graph.Nodes
            .Select(x => new NodeDegreesVM
            {
                Model = x,
                Caption = captions.Get(x.Id).DisplayText,
                In = x.In.Count,
                Out = x.Out.Count,
                Total = x.In.Count + x.Out.Count
            })
            .OrderByDescending(x => x.Total)
            .ToList();
    }

    private static GraphDensityVM ComputeGraphDensity(IModuleRepository modules, IGraph graph) =>
        new()
        {
            NodeCount = graph.Nodes.Count,
            EdgeCount = graph.Edges.Count,
            Density = GraphMetricsCalculator.ComputeGraphDensity(graph)
        };

    private static IReadOnlyCollection<CycleVM> ComputeCycles(IModuleRepository modules, IGraph graph)
    {
        var captions = modules.GetPropertySetFor<Caption>();

        CycleVM CreateCycleVM(Cycle cycle) =>
            new()
            {
                Model = cycle,
                Start = captions.Get(cycle.Start.Id).DisplayText,
                Path = cycle.Path.Skip(1).Select(n => captions.Get(n.Id).DisplayText).ToList()
            };

        return CycleFinder.FindAllCycles(graph)
            .Select(CreateCycleVM)
            .ToList();
    }

    private static PathwaysVM ComputePathways(IModuleRepository modules, IGraph graph)
    {
        var shortestPaths = ShortestPathsFinder.FindAllShortestPaths(graph);

        var captions = modules.GetPropertySetFor<Caption>();

        return new()
        {
            Diameter = GraphMetricsCalculator.ComputeDiameter(shortestPaths),
            AveragePathLength = GraphMetricsCalculator.ComputeAveragePathLength(graph, shortestPaths),
            BetweennessCentrality = GraphMetricsCalculator.ComputeBetweennessCentrality(graph, shortestPaths)
                .Select(x => new GraphItemMeasurementVM
                {
                    Model = x.Owner,
                    Caption = captions.Get(x.Owner.Id).DisplayText,
                    Absolute = x.Absolute,
                    Normalized = x.Normalized,
                })
                .OrderByDescending(x => x.Absolute)
                .ToList(),
            EdgeBetweenness = GraphMetricsCalculator.ComputeEdgeBetweenness(graph, shortestPaths)
                .Select(x => new GraphItemMeasurementVM
                {
                    Model = x.Owner,
                    Caption = $"{captions.Get(x.Owner.Source.Id).DisplayText} -> {captions.Get(x.Owner.Target.Id).DisplayText}",
                    Absolute = x.Absolute,
                    Normalized = x.Normalized,
                })
                .OrderByDescending(x => x.Absolute)
                .ToList(),
            ClosenessCentrality = GraphMetricsCalculator.ComputeClosenessCentrality(graph, shortestPaths)
                .Select(x => new GraphItemMeasurementVM
                {
                    Model = x.Owner,
                    Caption = captions.Get(x.Owner.Id).DisplayText,
                    Absolute = x.Absolute,
                    Normalized = x.Normalized,
                })
                .OrderByDescending(x => x.Absolute)
                .ToList()
        };
    }

    internal void Closed()
    {
        myFinishAction = null;
    }
}

