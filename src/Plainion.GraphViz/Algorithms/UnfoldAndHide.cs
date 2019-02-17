using System.Collections.Generic;
using System.Linq;
using Plainion.GraphViz.Model;
using Plainion.GraphViz.Presentation;

namespace Plainion.GraphViz.Algorithms
{
    public class UnfoldAndHide
    {
        private readonly IGraphPresentation myPresentation;

        public UnfoldAndHide(IGraphPresentation presentation)
        {
            Contract.RequiresNotNull(presentation, nameof(presentation));

            myPresentation = presentation;
        }

        public void Execute(Cluster cluster, SiblingsType siblingsType)
        {
            Contract.Requires(siblingsType != SiblingsType.None, "SiblingsType.None not supported");

            var transformations = myPresentation.GetModule<ITransformationModule>();
            var transformation = transformations.Items
                .OfType<ClusterFoldingTransformation>()
                .SingleOrDefault();

            if (transformation == null)
            {
                return;
            }

            if (!transformation.Clusters.Contains(cluster.Id))
            {
                return;
            }

            var clusterNodeId = transformation.GetClusterNodeId(cluster.Id);
            var clusterNode = transformations.Graph.Nodes.Single(n => n.Id == clusterNodeId);
            var referencingNodes = new HashSet<string>(GetVisibleSiblings(clusterNode));

            // unfold
            transformation.Toggle(cluster.Id);

            var unfoldedCluster = transformations.Graph.Clusters.Single(c => c.Id == cluster.Id);

            // so far the unfoldedCluster nodes are NOT visible ... if we have used s.th. like "show siblings"
            // on the cluster node!
            var referencedNodes = unfoldedCluster.Nodes
                .Where(n =>
                {
                    var nodes = Enumerable.Empty<string>();
                    if (siblingsType == SiblingsType.Any || siblingsType == SiblingsType.Sources)
                    {
                        nodes = nodes.Concat(n.Out.Select(e => e.Target.Id));
                    }
                    if (siblingsType == SiblingsType.Any || siblingsType == SiblingsType.Targets)
                    {
                        nodes = nodes.Concat(n.In.Select(e => e.Source.Id));
                    }
                    return nodes.Any(referencingNodes.Contains);
                });

            var mask = new NodeMask();
            mask.Set(referencedNodes);

            var caption = myPresentation.GetPropertySetFor<Caption>().Get(cluster.Id);
            var maskType = siblingsType == SiblingsType.Any ? "Siblings" : (siblingsType == SiblingsType.Sources ? "Sources" : "Targets");
            mask.Label = maskType + " of " + caption.DisplayText;

            var module = myPresentation.GetModule<INodeMaskModule>();
            module.Push(mask);
        }

        private IEnumerable<string> GetVisibleSiblings(Node node)
        {
            return node.In
                .Where(e => myPresentation.Picking.Pick(e))
                .Select(e => e.Source)
                .Concat(node.Out
                    .Where(e => myPresentation.Picking.Pick(e))
                    .Select(e => e.Target))
                .Where(n => myPresentation.Picking.Pick(n))
                .Select(n => n.Id);
        }
    }
}
