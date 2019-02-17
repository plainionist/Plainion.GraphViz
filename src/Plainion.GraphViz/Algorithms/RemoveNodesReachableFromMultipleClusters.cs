using System.Collections.Generic;
using System.Linq;

using Plainion.GraphViz.Model;
using Plainion.GraphViz.Presentation;

namespace Plainion.GraphViz.Algorithms
{
    public class RemoveNodesReachableFromMultipleClusters : AbstractAlgorithm
    {
        public RemoveNodesReachableFromMultipleClusters(IGraphPresentation presentation)
            :base(presentation)
        {
        }

        public INodeMask Comppute()
        {
            var mask = new NodeMask();
            mask.Label = "Nodes reachable by multiple clusters";
            mask.IsShowMask = false;

            var transformationModule = Presentation.GetModule<ITransformationModule>();

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

            foreach (var edge in Traverse.BreathFirst(cache.Keys, source => source.Out.Where(e => Presentation.Picking.Pick(e.Target))))
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

            return mask;
        }
    }
}
