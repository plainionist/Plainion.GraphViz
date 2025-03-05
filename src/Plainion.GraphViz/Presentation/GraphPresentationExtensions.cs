using System.Collections.Generic;
using System.Linq;
using Plainion.Graphs;

namespace Plainion.GraphViz.Presentation;

public static class GraphPresentationExtensions
{
    public static DynamicClusterTransformation DynamicClusters(this IGraphPresentation self)
    {
        Contract.RequiresNotNull(self, nameof(self));

        var transformations = self.GetModule<ITransformationModule>();
        return transformations.Items
            .OfType<DynamicClusterTransformation>()
            .Single();
    }

    public static void ToogleFoldingOfVisibleClusters(this IGraphPresentation self)
    {
        Contract.RequiresNotNull(self, nameof(self));

        var clusterFolding = self.ClusterFolding;

        var visibleClusters = self.TransformedGraph.Clusters
            .Where(self.Picking.Pick)
            .Select(c => c.Id)
            .ToList();

        // any visible cluster folded?
        if (visibleClusters.Any(clusterFolding.Clusters.Contains))
        {
            // safe to pass nodes not known to transformation
            clusterFolding.Remove(visibleClusters);
        }
        else
        {
            clusterFolding.Add(visibleClusters);
        }
    }

    public static INodeMaskModule Masks(this IGraphPresentation self) =>
        self.GetModule<INodeMaskModule>();

    public static void Select(this IGraphPresentation self, Node node, SiblingsType role)
    {
        var selection = self.GetPropertySetFor<Selection>();
        foreach (var e in GetEdges(node, role).Where(self.Picking.Pick))
        {
            selection.Select(e);
        }
    }

    private static IEnumerable<Edge> GetEdges(Node node, SiblingsType type)
    {
        if (type == SiblingsType.Sources || type == SiblingsType.Any)
        {
            foreach (var e in node.In) yield return e;
        }

        if (type == SiblingsType.Targets || type == SiblingsType.Any)
        {
            foreach (var e in node.Out) yield return e;
        }
    }

    public static void Select(this IPropertySetModule<Selection> self, Edge edge)
    {
        self.Get(edge.Id).IsSelected = true;
        self.Get(edge.Source.Id).IsSelected = true;
        self.Get(edge.Target.Id).IsSelected = true;
    }
}

