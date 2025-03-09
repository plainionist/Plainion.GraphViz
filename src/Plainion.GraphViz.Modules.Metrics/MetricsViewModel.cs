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
    private IModuleChangedObserver myNodeMaskObserver;
    private IModuleChangedObserver myTransformationsObserver;
    private int myDiameter;
    private double myAveragePathLength;
    private IReadOnlyCollection<GraphItemMeasurementVM> myBetweennessCentrality;
    private IReadOnlyCollection<GraphItemMeasurementVM> myEdgeBetweenness;
    private IReadOnlyCollection<GraphItemMeasurementVM> myClosenessCentrality;

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
            foreach (var edge in cycleVM.Model.Edges)
            {
                selection.Select(edge);
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
            else if (itemMeasurementVM.Model is Graphs.Undirected.Node uNode)
            {
                selection.Get(uNode.Id).IsSelected = true;
                myEventAggregator.GetEvent<NodeFocusedEvent>().Publish(uNode);
            }
            else if (itemMeasurementVM.Model is Edge edge)
            {
                selection.Select(edge);

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

    public int Diameter
    {
        get { return myDiameter; }
        set { SetProperty(ref myDiameter, value); }
    }

    public double AveragePathLength
    {
        get { return myAveragePathLength; }
        set { SetProperty(ref myAveragePathLength, value); }
    }

    public IReadOnlyCollection<GraphItemMeasurementVM> BetweennessCentrality
    {
        get { return myBetweennessCentrality; }
        set { SetProperty(ref myBetweennessCentrality, value); }
    }

    public IReadOnlyCollection<GraphItemMeasurementVM> EdgeBetweenness
    {
        get { return myEdgeBetweenness; }
        set { SetProperty(ref myEdgeBetweenness, value); }
    }

    public IReadOnlyCollection<GraphItemMeasurementVM> ClosenessCentrality
    {
        get { return myClosenessCentrality; }
        set { SetProperty(ref myClosenessCentrality, value); }
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

        var graph = BuildVisibleGraph();
        var captions = Model.Presentation.GetPropertySetFor<Caption>();

        Step(() => { DegreeCentrality = ComputeDegreeCentrality(captions, graph); });
        Step(() => { GraphDensity = ComputeGraphDensity(graph); });
        Step(() => { Cycles = ComputeCycles(captions, graph); });

        var shortestPaths = ShortestPathsFinder.FindAllShortestPaths(graph);

        Step(() => { Diameter = GraphMetrics.ComputeDiameter(shortestPaths); });
        Step(() => { AveragePathLength = GraphMetrics.ComputeAveragePathLength(graph, shortestPaths); });
        Step(() => { BetweennessCentrality = ComputeBetweennessCentrality(captions, graph, shortestPaths); });
        Step(() => { EdgeBetweenness = ComputeEdgeBetweenness(captions, graph, shortestPaths); });

        var undirectedGraph = Graphs.Undirected.RelaxedGraphBuilder.Convert(graph);
        var shortestUndirectedPaths = UndirectedShortestPathsFinder.FindAllShortestPaths(undirectedGraph);

        Step(() => { ClosenessCentrality = ComputeClosenessCentrality(captions, undirectedGraph, shortestUndirectedPaths); });
    }

    private IGraph BuildVisibleGraph()
    {
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

        return builder.Graph;
    }

    private static IReadOnlyCollection<NodeDegreesVM> ComputeDegreeCentrality(IPropertySetModule<Caption> captions, IGraph graph) =>
        graph.Nodes
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

    private static GraphDensityVM ComputeGraphDensity(IGraph graph) =>
        new()
        {
            NodeCount = graph.Nodes.Count,
            EdgeCount = graph.Edges.Count,
            Density = GraphMetrics.ComputeGraphDensity(graph)
        };

    private static IReadOnlyCollection<CycleVM> ComputeCycles(IPropertySetModule<Caption> captions, IGraph graph)
    {
        CycleVM CreateCycleVM(Cycle cycle) =>
            new()
            {
                Model = cycle,
                Start = captions.Get(cycle.Start.Id).DisplayText,
                Path = cycle.Path.Select(n => captions.Get(n.Id).DisplayText).ToList()
            };

        return CycleFinder.FindAllCycles(graph)
            .Select(CreateCycleVM)
            .ToList();
    }

    private static IReadOnlyCollection<GraphItemMeasurementVM> ComputeBetweennessCentrality(IPropertySetModule<Caption> captions, IGraph graph, ShortestPaths shortestPaths) =>
        GraphMetrics.ComputeBetweennessCentrality(graph, shortestPaths)
            .Select(x => new GraphItemMeasurementVM
            {
                Model = x.Owner,
                Caption = captions.Get(x.Owner.Id).DisplayText,
                Absolute = x.Absolute,
                Normalized = x.Normalized,
            })
            .OrderByDescending(x => x.Absolute)
            .ToList();

    private static IReadOnlyCollection<GraphItemMeasurementVM> ComputeEdgeBetweenness(IPropertySetModule<Caption> captions, IGraph graph, ShortestPaths shortestPaths)=>
        GraphMetrics.ComputeEdgeBetweenness(graph, shortestPaths)
            .Select(x => new GraphItemMeasurementVM
            {
                Model = x.Owner,
                Caption = $"{captions.Get(x.Owner.Source.Id).DisplayText} -> {captions.Get(x.Owner.Target.Id).DisplayText}",
                Absolute = x.Absolute,
                Normalized = x.Normalized,
            })
            .OrderByDescending(x => x.Absolute)
            .ToList();

    private static IReadOnlyCollection<GraphItemMeasurementVM> ComputeClosenessCentrality(IPropertySetModule<Caption> captions, IReadOnlyCollection<Graphs.Undirected.Node> graph, ShortestUndirectedPaths shortestPaths) =>
        UndirectedGraphMetrics.ComputeClosenessCentrality(graph, shortestPaths)
            .Select(x => new GraphItemMeasurementVM
            {
                Model = x.Owner,
                Caption = captions.Get(x.Owner.Id).DisplayText,
                Absolute = x.Absolute,
                Normalized = x.Normalized,
            })
            .OrderByDescending(x => x.Absolute)
            .ToList();

    internal void Closed()
    {
        myFinishAction = null;
    }
}

