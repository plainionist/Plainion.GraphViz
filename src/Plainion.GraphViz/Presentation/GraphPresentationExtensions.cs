using System;
using System.Collections.Generic;
using System.Linq;

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
    }
}
