using System;
using System.Linq;
using Plainion.GraphViz.Presentation;

namespace Plainion.GraphViz.Algorithms
{
    public class ChangeClusterFolding
    {
        private readonly IGraphPresentation myPresentation;

        public ChangeClusterFolding(IGraphPresentation presentation)
        {
            Contract.RequiresNotNull(presentation, "presentation");

            myPresentation = presentation;
        }

        public void Execute(Action<ClusterFoldingTransformation> action)
        {
            var transformations = myPresentation.GetModule<ITransformationModule>();
            var transformation = transformations.Items
                .OfType<ClusterFoldingTransformation>()
                .SingleOrDefault();

            if (transformation == null)
            {
                transformation = new ClusterFoldingTransformation(myPresentation);

                action(transformation);

                transformations.Add(transformation);
            }
            else
            {
                action(transformation);
            }
        }

        public void FoldUnfoldAllClusters()
        {
            var transformations = myPresentation.GetModule<ITransformationModule>();
            var transformation = transformations.Items
                .OfType<ClusterFoldingTransformation>()
                .SingleOrDefault();

            if (transformation == null)
            {
                transformation = new ClusterFoldingTransformation(myPresentation);

                foreach (var cluster in myPresentation.Graph.Clusters)
                {
                    transformation.Add(cluster.Id);
                }

                transformations.Add(transformation);
            }
            else
            {
                if (transformation.ClusterToClusterNodeMapping.Any())
                {
                    foreach (var cluster in transformation.ClusterToClusterNodeMapping.Keys.ToList())
                    {
                        transformation.Remove(cluster);
                    }
                }
                else
                {
                    foreach (var cluster in myPresentation.Graph.Clusters)
                    {
                        transformation.Add(cluster.Id);
                    }
                }
            }
        }
    }
}
