using System.Collections.Generic;
using System.Linq;
using Plainion.GraphViz.Algorithms;
using Plainion.GraphViz.Model;

namespace Plainion.GraphViz.Presentation
{
    public static class GraphPresentationExtensions
    {
        public static DynamicClusterTransformation DynamicClusters(this IGraphPresentation presentation)
        {
            Contract.RequiresNotNull(presentation, nameof(presentation));

            var transformations = presentation.GetModule<ITransformationModule>();
            return transformations.Items
                .OfType<DynamicClusterTransformation>()
                .Single();
        }

        public static ClusterFoldingTransformation ClusterFolding(this IGraphPresentation presentation)
        {
            var transformations = presentation.GetModule<ITransformationModule>();
            var transformation = transformations.Items
                .OfType<ClusterFoldingTransformation>()
                .SingleOrDefault();

            if (transformation == null)
            {
                transformation = new ClusterFoldingTransformation(presentation);
                transformations.Add(transformation);
            }

            return transformation;
        }

        public static void ToogleFoldingOfVisibleClusters(this IGraphPresentation presentation)
        {
            Contract.RequiresNotNull(presentation, nameof(presentation));

            var transformation = presentation.ClusterFolding();

            var visibleClusters = presentation.TransformedGraph().Clusters
                .Where(presentation.Picking.Pick)
                .Select(c => c.Id)
                .ToList();

            // any visible cluster folded?
            if (visibleClusters.Any(transformation.Clusters.Contains))
            {
                // safe to pass nodes not known to transformation
                transformation.Remove(visibleClusters);
            }
            else
            {
                transformation.Add(visibleClusters);
            }
        }

        public static INodeMaskModule Masks(this IGraphPresentation presentation)
        {
            return presentation.GetModule<INodeMaskModule>();
        }

        public static IGraph TransformedGraph(this IGraphPresentation presentation)
        {
            return presentation.GetModule<ITransformationModule>().Graph;
        }

        public static void Select(this IGraphPresentation presentation, Node node, SiblingsType role)
        {
            var selection = presentation.GetPropertySetFor<Selection>();
            foreach (var e in GetEdges(node, role).Where(presentation.Picking.Pick))
            {
                selection.Get(e.Id).IsSelected = true;
                selection.Get(e.Source.Id).IsSelected = true;
                selection.Get(e.Target.Id).IsSelected = true;
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
    }
}
