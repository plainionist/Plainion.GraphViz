using System.Collections.Generic;
using System.Linq;

using Plainion.GraphViz.Model;
using Plainion.GraphViz.Presentation;

namespace Plainion.GraphViz.Algorithms
{
    /// <summary>
    /// Shows all nodes which are only reachable from a single cluster
    /// </summary>
    public class RemoveNodesReachableFromMultipleClusters
    {
        private readonly IGraphPresentation myPresentation;

        public RemoveNodesReachableFromMultipleClusters(IGraphPresentation presentation)
        {
            Contract.RequiresNotNull(presentation, nameof(presentation));

            myPresentation = presentation;
        }

        public void Execute()
        {
            var mask = new NodeMask();
            mask.Label = "Nodes reachable by multiple cluster";
            mask.IsShowMask = false;

            var transformationModule = myPresentation.GetModule<ITransformationModule>();

            // nodeId -> clusterId
            var cache = new Dictionary<Node, string>();

            // add starts
            var clusterNodes = new HashSet<string>();
            foreach (var cluster in transformationModule.Graph.Clusters)
            {
                foreach (var clusterNode in cluster.Nodes)
                {
                    cache[clusterNode] = cluster.Id;
                    clusterNodes.Add(clusterNode.Id);
                }
            }

            foreach (var edge in Traverse.BreathFirst(cache.Keys, source => source.Out.Where(e => myPresentation.Picking.Pick(e.Target))))
            {
                // do not hide cluster nodes
                if (clusterNodes.Contains(edge.Target.Id))
                {
                    continue;
                }

                var sourceClusterId = cache[edge.Source];

                if (sourceClusterId == null)
                {
                    // on multiple path
                    mask.Set(edge.Target);
                    cache[edge.Target] = null;
                    continue;
                }

                string existingClusterId;
                if (cache.TryGetValue(edge.Target, out existingClusterId) && existingClusterId != sourceClusterId)
                {
                    // multiple path detected
                    mask.Set(edge.Target);
                    cache[edge.Target] = null;
                }
                else
                {
                    // first path found
                    cache[edge.Target] = sourceClusterId;
                }
            }

            var module = myPresentation.GetModule<INodeMaskModule>();
            module.Push(mask);
        }
    }
}
