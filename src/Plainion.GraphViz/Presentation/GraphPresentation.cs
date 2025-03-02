using System;
using System.Collections.Generic;
using System.Linq;
using Plainion.Graphs;
using Plainion.GraphViz.Dot;

namespace Plainion.GraphViz.Presentation;

public class GraphPresentation : IGraphPresentation
{
    private List<object> myModules;
    private IGraph myGraph;
    private IModuleChangedObserver myNodeMaskModuleObserver;
    private IModuleChangedObserver myTransformationModuleObserver;

    public GraphPresentation(IGraph graph = null)
    {
        myModules =
        [
            new PropertySetModule<ToolTipContent>(id => new ToolTipContent(id, null)),
            new PropertySetModule<Selection>(id => new Selection(id) { IsSelected = false }),
            // TODO: move translation from string to WPF entities to parser
            new PropertySetModule<NodeStyle>(id => new NodeStyle(id)),
            new PropertySetModule<EdgeStyle>(id => new EdgeStyle(id)),
            new CaptionModule(id => new Caption(id, null)),
            new GraphLayoutModule(),
            new NodeMaskModule(),
            new TransformationModule(this),
            new GraphAttributesModule()
        ];

        Picking = new PickingCache(this, new GraphPicking(this));

        myNodeMaskModuleObserver = GetModule<INodeMaskModule>().CreateObserver();
        myNodeMaskModuleObserver.ModuleChanged += OnModuleChanged;

        myTransformationModuleObserver = GetModule<ITransformationModule>().CreateObserver();
        myTransformationModuleObserver.ModuleChanged += OnModuleChanged;

        if (graph != null)
        {
            Graph = graph;
        }
    }

    private void OnModuleChanged(object sender, EventArgs e)
    {
        GraphVisibilityChanged?.Invoke(this, EventArgs.Empty);
    }

    public IPropertySetModule<T> GetPropertySetFor<T>() where T : AbstractPropertySet
    {
        return myModules.OfType<PropertySetModule<T>>().SingleOrDefault();
    }

    public T GetModule<T>()
    {
        return myModules.OfType<T>().SingleOrDefault();
    }

    public IGraph Graph
    {
        get
        {
            return myGraph;
        }
        set
        {
            if (myGraph != null)
            {
                throw new InvalidOperationException("Graph already set to presentation. Changing Graph is not allowed!");
            }

            if (value is Graph g && g.IsFrozen)
            {
                myGraph = g;
            }
            else
            {
                // graph is mutable in order to support easy graph building -> protect against graph changes
                myGraph = Objects.Clone(value);
            }

            // fill up the Captions module here directly to avoid performance issues by
            // "on demand" adding which then fires "changed" events
            FillCaptions();

            DetectOptimalSettings();
        }
    }

    private void DetectOptimalSettings()
    {
        // Automatically fold all clusters if more than 500 nodes (random bigger number)
        if (myGraph.Nodes.Count() > 500)
        {
            this.ToogleFoldingOfVisibleClusters();
        }

        // A hierarchical layout does not perform well when the graph exceeds certain node limit.
        // Let's choose something more performant. Node limit for historical reasons: 300
        GetModule<IGraphLayoutModule>().Algorithm = myGraph.Nodes.Count() > 300
            ? LayoutAlgorithm.ScalableForceDirectedPlancement
            : LayoutAlgorithm.Hierarchy;
    }

    private void FillCaptions()
    {
        var captions = GetPropertySetFor<Caption>();

        foreach (var x in myGraph.Nodes)
        {
            captions.Get(x.Id);
        }
        foreach (var x in myGraph.Edges)
        {
            captions.Get(x.Id);
        }
        foreach (var x in myGraph.Clusters)
        {
            captions.Get(x.Id);
        }
    }

    public IGraphPicking Picking { get; private set; }

    public void InvalidateLayout()
    {
        GetModule<IGraphLayoutModule>().Clear();
    }

    public event EventHandler GraphVisibilityChanged;

    public void Dispose()
    {
        if (myModules != null)
        {
            myNodeMaskModuleObserver.ModuleChanged -= OnModuleChanged;
            myNodeMaskModuleObserver.Dispose();

            myTransformationModuleObserver.ModuleChanged -= OnModuleChanged;
            myTransformationModuleObserver.Dispose();

            GraphVisibilityChanged = null;

            myModules.Clear();
            myModules = null;
        }
    }

    public IGraphPresentation UnionWith(IGraphPresentation other, Func<IGraphPresentation> presentationCreator)
    {
        var result = presentationCreator();

        var builder = new RelaxedGraphBuilder();

        foreach (var node in GetNodes(this, other))
        {
            builder.TryAddNode(node.Id);
        }

        foreach (var edge in GetEdges(this, other))
        {
            builder.TryAddEdge(edge.Source.Id, edge.Target.Id, edge.Weight);
        }

        foreach (var cluster in GetClusters(this, other))
        {
            builder.TryAddCluster(cluster.Id, cluster.Nodes.Select(n => n.Id));
        }

        result.Graph = builder.Graph;

        UnionWith<ToolTipContent>(this, other, result);
        UnionWith<Selection>(this, other, result);
        UnionWith<NodeStyle>(this, other, result);
        UnionWith<EdgeStyle>(this, other, result);
        UnionWith<Caption>(this, other, result);

        {
            var resultModule = result.GetModule<INodeMaskModule>();

            foreach (var item in GetModule<INodeMaskModule>().Items.Concat(other.GetModule<INodeMaskModule>().Items))
            {
                resultModule.Push(item);
            }
        }

        return result;
    }

    private static IEnumerable<Node> GetNodes(GraphPresentation lhs, IGraphPresentation rhs)
    {
        if (lhs.Graph == null) return rhs.Graph.Nodes;
        if (rhs.Graph == null) return lhs.Graph.Nodes;

        return lhs.Graph.Nodes.Concat(rhs.Graph.Nodes);
    }

    private static IEnumerable<Edge> GetEdges(GraphPresentation lhs, IGraphPresentation rhs)
    {
        if (lhs.Graph == null) return rhs.Graph.Edges;
        if (rhs.Graph == null) return lhs.Graph.Edges;

        return lhs.Graph.Edges.Concat(rhs.Graph.Edges);
    }

    private static IEnumerable<Cluster> GetClusters(GraphPresentation lhs, IGraphPresentation rhs)
    {
        if (lhs.Graph == null) return rhs.Graph.Clusters;
        if (rhs.Graph == null) return lhs.Graph.Clusters;

        return lhs.Graph.Clusters.Concat(rhs.Graph.Clusters);
    }

    private static void UnionWith<T1>(IGraphPresentation lhs, IGraphPresentation rhs, IGraphPresentation result) where T1 : AbstractPropertySet
    {
        var resultModule = result.GetPropertySetFor<T1>();

        foreach (var item in lhs.GetPropertySetFor<T1>().Items)
        {
            resultModule.Add(item);
        }

        foreach (var item in rhs.GetPropertySetFor<T1>().Items)
        {
            if (!resultModule.Items.Any(i => i.OwnerId == item.OwnerId))
            {
                resultModule.Add(item);
            }
        }
    }
}
