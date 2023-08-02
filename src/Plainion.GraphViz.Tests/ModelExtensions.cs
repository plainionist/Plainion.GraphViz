using System.Linq;
using Plainion.GraphViz.Model;
using Plainion.GraphViz.Presentation;

namespace Plainion.GraphViz.Tests
{
    public static class ModelExtensions
    {
        public static Node GetNode(this IGraphPresentation self, string id) =>
            self.Graph.Nodes.Single(x => x.Id == id);

        public static Cluster GetCluster(this IGraphPresentation self, string id) =>
            self.Graph.Clusters.Single(x => x.Id == id);
    }
}
