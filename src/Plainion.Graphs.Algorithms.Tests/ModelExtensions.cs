using System.Linq;
using Plainion.Graphs.Projections;

namespace Plainion.Graphs.Algorithms.Tests;

public static class ModelExtensions
{
    public static Node GetNode(this IGraphProjections self, string id) =>
        self.Graph.Nodes.Single(x => x.Id == id);

    public static Cluster GetCluster(this IGraphProjections self, string id) =>
        self.Graph.Clusters.Single(x => x.Id == id);
}
