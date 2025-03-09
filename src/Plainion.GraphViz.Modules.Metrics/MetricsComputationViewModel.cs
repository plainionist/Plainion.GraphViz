using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using Plainion.Graphs;
using Plainion.GraphViz.Modules.Metrics.Algorithms;
using Plainion.GraphViz.Presentation;
using Plainion.Windows.Mvvm;

namespace Plainion.GraphViz.Modules.Metrics;

class MetricsComputationViewModel : BindableBase
{
    private CancellationTokenSource myCTS;
    private IReadOnlyCollection<NodeDegreesVM> myDegreeCentrality;
    private GraphDensityVM myGraphDensity;
    private IReadOnlyCollection<CycleVM> myCycles;
    private int myDiameter;
    private double myAveragePathLength;
    private IReadOnlyCollection<GraphItemMeasurementVM> myBetweennessCentrality;
    private IReadOnlyCollection<GraphItemMeasurementVM> myEdgeBetweenness;
    private IReadOnlyCollection<GraphItemMeasurementVM> myClosenessCentrality;

    public MetricsComputationViewModel()
    {
        Init();
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

    public void RestartAnalysis(IGraphPresentation presentation)
    {
        Init();

        myCTS?.Cancel();

        TriggerAnalysis(presentation);
    }

    private void Init()
    {
        myDegreeCentrality = [];
        myCycles = [];
        myBetweennessCentrality = [];
        myEdgeBetweenness = [];
        myClosenessCentrality = [];
    }

    public void TriggerAnalysis(IGraphPresentation presentation)
    {
        if (myDegreeCentrality.Count != 0)
        {
            // results already available
            return;
        }

        if (presentation == null)
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

        Task.Run(() => RunAnalysis(presentation, myCTS.Token), myCTS.Token)
            .ContinueWith(_ => { myCTS = null; });
    }

    private void RunAnalysis(IGraphPresentation presentation, CancellationToken token)
    {
        void Step(Action action)
        {
            Application.Current.Dispatcher.BeginInvoke(action);
            token.ThrowIfCancellationRequested();
        }

        var graph = BuildVisibleGraph(presentation);
        var captions = presentation.GetPropertySetFor<Caption>();

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

    private static IGraph BuildVisibleGraph(IGraphPresentation presentation)
    {
        var builder = new RelaxedGraphBuilder();

        foreach (var node in presentation.TransformedGraph.Nodes.Where(presentation.Picking.Pick))
        {
            builder.TryAddNode(node.Id);
        }

        foreach (var edge in presentation.TransformedGraph.Edges.Where(presentation.Picking.Pick))
        {
            builder.TryAddEdge(edge.Source.Id, edge.Target.Id);
        }

        foreach (var cluster in presentation.TransformedGraph.Clusters.Where(presentation.Picking.Pick))
        {
            builder.TryAddCluster(cluster.Id, cluster.Nodes.Where(presentation.Picking.Pick).Select(x => x.Id));
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
}

