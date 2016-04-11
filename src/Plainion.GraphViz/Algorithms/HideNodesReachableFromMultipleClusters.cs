using System.Collections.Generic;
using System.Linq;

using Plainion.GraphViz.Model;
using Plainion.GraphViz.Presentation;

namespace Plainion.GraphViz.Algorithms
{
    /// <summary>
    /// Shows all nodes which are only reachable from a single cluster
    /// </summary>
    public class HideNodesReachableFromMultipleClusters
    {
        private readonly IGraphPresentation myPresentation;

        public HideNodesReachableFromMultipleClusters(IGraphPresentation presentation)
        {
            Contract.RequiresNotNull(presentation, "presentation");

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

            var newNodes = new HashSet<Node>(cache.Keys);

            while (newNodes.Count > 0)
            {
                var nodes = newNodes.ToList();
                newNodes.Clear();

                foreach (var parent in nodes)
                {
                    var parentClusterId = cache[parent];

                    foreach (var target in parent.Out.Select(e => e.Target))
                    {
                        if (clusterNodes.Contains(target.Id))
                        {
                            // do not hide cluster nodes
                            continue;
                        }

                        newNodes.Add(target);

                        if (parentClusterId == null)
                        {
                            // on multiple path
                            mask.Set(target);
                            cache[target] = null;
                            continue;
                        }

                        string existingClusterId;
                        if (cache.TryGetValue(target, out existingClusterId)
                            && (existingClusterId != parentClusterId || existingClusterId == null))
                        {
                            // multiple path detected
                            mask.Set(target);
                            cache[target] = null;
                        }
                        else
                        {
                            // first path found
                            cache[target] = parentClusterId;
                        }
                    }
                }

                newNodes.ExceptWith(cache.Keys);
            }

            var module = myPresentation.GetModule<INodeMaskModule>();
            module.Push(mask);
        }
    }
}
