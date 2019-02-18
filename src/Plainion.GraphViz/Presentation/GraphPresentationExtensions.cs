using System;
using System.Collections.Generic;
using System.Linq;
using Plainion.GraphViz.Algorithms;
using Plainion.GraphViz.Model;

namespace Plainion.GraphViz.Presentation
{
    public static class GraphPresentationExtensions
    {
        public static void ChangeClusterAssignment(this IGraphPresentation presentation, Action<DynamicClusterTransformation> action)
        {
            Contract.RequiresNotNull(presentation, nameof(presentation));

            var transformations = presentation.GetModule<ITransformationModule>();
            var transformation = transformations.Items
                .OfType<DynamicClusterTransformation>()
                .Single();

            action(transformation);
        }

        public static void AddToCluster(this IGraphPresentation presentation, IReadOnlyCollection<string> nodes, string cluster)
        {
            presentation.ChangeClusterAssignment(t => t.AddToCluster(nodes, cluster));
        }

        public static void ChangeClusterFolding(this IGraphPresentation presentation, Action<ClusterFoldingTransformation> action)
        {
            Contract.RequiresNotNull(presentation, nameof(presentation));

            var transformations = presentation.GetModule<ITransformationModule>();
            var transformation = transformations.Items
                .OfType<ClusterFoldingTransformation>()
                .SingleOrDefault();

            if (transformation == null)
            {
                transformation = new ClusterFoldingTransformation(presentation);

                action(transformation);

                transformations.Add(transformation);
            }
            else
            {
                action(transformation);
            }
        }

        public static void FoldUnfoldAllClusters(this IGraphPresentation presentation)
        {
            Contract.RequiresNotNull(presentation, nameof(presentation));

            var transformations = presentation.GetModule<ITransformationModule>();
            var transformation = transformations.Items
                .OfType<ClusterFoldingTransformation>()
                .SingleOrDefault();

            if (transformation == null)
            {
                transformation = new ClusterFoldingTransformation(presentation);

                foreach (var cluster in presentation.Graph.Clusters)
                {
                    transformation.Add(cluster.Id);
                }

                transformations.Add(transformation);
            }
            else
            {
                if (transformation.Clusters.Any())
                {
                    transformation.Remove(transformation.Clusters.ToList());
                }
                else
                {
                    transformation.Add(presentation.Graph.Clusters.Select(c => c.Id));
                }
            }
        }

        public static void AddMask(this IGraphPresentation presentation, INodeMask mask)
        {
            presentation.GetModule<INodeMaskModule>().Push(mask);
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
