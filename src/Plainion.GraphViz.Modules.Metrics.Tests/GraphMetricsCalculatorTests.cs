using System.Linq;
using NUnit.Framework;
using Plainion.Graphs;
using Plainion.GraphViz.Modules.Metrics.Algorithms;

namespace Plainion.GraphViz.Modules.Metrics.Tests;

[TestFixture]
public class GraphMetricsCalculatorTests
{
    [Test]
    public void EmptyGraph_ReturnsZeros()
    {
        var builder = new RelaxedGraphBuilder();

        var paths = ShortestPathsFinder.FindAllShortestPaths(builder.Graph);

        Assert.That(GraphMetricsCalculator.ComputeGraphDensity(builder.Graph), Is.EqualTo(0));
        Assert.That(GraphMetricsCalculator.ComputeDiameter(paths), Is.EqualTo(0));
        Assert.That(GraphMetricsCalculator.ComputeAveragePathLength(builder.Graph, paths), Is.EqualTo(0.0));

        var betweenness = GraphMetricsCalculator.ComputeBetweennessCentrality(builder.Graph, paths);
        Assert.That(betweenness.All(x => x.Absolute == 0.0), Is.True);

        var edgeBetweenness = GraphMetricsCalculator.ComputeEdgeBetweenness(builder.Graph, paths);
        Assert.That(edgeBetweenness.All(kv => kv.Absolute == 0.0), Is.True);
    }

    [Test]
    public void SingleNodeGraph_ReturnsZeros()
    {
        var builder = new RelaxedGraphBuilder();
        builder.TryAddNode("A"); 

        var paths = ShortestPathsFinder.FindAllShortestPaths(builder.Graph);

        Assert.That(GraphMetricsCalculator.ComputeGraphDensity(builder.Graph), Is.EqualTo(0)); // 0 / (1 * 0) = 0
        Assert.That(GraphMetricsCalculator.ComputeDiameter(paths), Is.EqualTo(0)); // No paths
        Assert.That(GraphMetricsCalculator.ComputeAveragePathLength(builder.Graph, paths), Is.EqualTo(0.0)); // No pairs

        var betweenness = GraphMetricsCalculator.ComputeBetweennessCentrality(builder.Graph, paths).ToDictionary(x => x.OwnerId);
        Assert.That(betweenness["A"].Absolute, Is.EqualTo(0.0)); // No paths
        Assert.That(betweenness["A"].Normalized, Is.EqualTo(0.0));

        var edgeBetweenness = GraphMetricsCalculator.ComputeEdgeBetweenness(builder.Graph, paths).ToDictionary(x => x.OwnerId, x => x.Absolute);
        Assert.That(edgeBetweenness.Count, Is.EqualTo(0)); // No edges
    }

    [Test]
    public void SingleEdgeGraph_DiameterAndAvgPath()
    {
        var builder = new RelaxedGraphBuilder();
        builder.TryAddEdge("A", "B");

        var paths = ShortestPathsFinder.FindAllShortestPaths(builder.Graph);

        Assert.That(GraphMetricsCalculator.ComputeGraphDensity(builder.Graph), Is.EqualTo(0.5));
        Assert.That(GraphMetricsCalculator.ComputeDiameter(paths), Is.EqualTo(1)); // A -> B
        Assert.That(GraphMetricsCalculator.ComputeAveragePathLength(builder.Graph, paths), Is.EqualTo(1.0 / 2.0)); // Only 1 path

        var betweenness = GraphMetricsCalculator.ComputeBetweennessCentrality(builder.Graph, paths).ToDictionary(x => x.OwnerId);
        Assert.That(betweenness["A"].Absolute, Is.EqualTo(0.0)); // No intermediate nodes
        Assert.That(betweenness["B"].Absolute, Is.EqualTo(0.0));

        var edgeBetweenness = GraphMetricsCalculator.ComputeEdgeBetweenness(builder.Graph, paths).ToDictionary(x => x.OwnerId);
        Assert.That(edgeBetweenness["edge-from-A-to-B"].Absolute, Is.EqualTo(1.0)); // A -> B path
        Assert.That(edgeBetweenness["edge-from-A-to-B"].Normalized, Is.EqualTo(0.0)); // maxPairs = 0, so normalized = 0
    }

    [Test]
    public void TriangleGraph_Metrics()
    {
        var builder = new RelaxedGraphBuilder();
        builder.TryAddEdge("A", "B");
        builder.TryAddEdge("B", "C");
        builder.TryAddEdge("C", "A");

        var paths = ShortestPathsFinder.FindAllShortestPaths(builder.Graph);

        Assert.That(GraphMetricsCalculator.ComputeGraphDensity(builder.Graph), Is.EqualTo(0.5));
        Assert.That(GraphMetricsCalculator.ComputeDiameter(paths), Is.EqualTo(2)); // e.g., A -> B -> C
        Assert.That(GraphMetricsCalculator.ComputeAveragePathLength(builder.Graph, paths), Is.EqualTo(1.5)); // 9 hops / 6 paths

        var betweenness = GraphMetricsCalculator.ComputeBetweennessCentrality(builder.Graph, paths).ToDictionary(x => x.OwnerId);
        Assert.That(betweenness["A"].Absolute, Is.EqualTo(1.0)); // C -> B
        Assert.That(betweenness["A"].Normalized, Is.EqualTo(1.0 / 2.0)); // C -> B
        Assert.That(betweenness["B"].Absolute, Is.EqualTo(1.0)); // A -> C
        Assert.That(betweenness["B"].Normalized, Is.EqualTo(1.0 / 2.0)); // A -> C
        Assert.That(betweenness["C"].Absolute, Is.EqualTo(1.0)); // B -> A
        Assert.That(betweenness["C"].Normalized, Is.EqualTo(1.0 / 2.0)); // B -> A

        var edgeBetweenness = GraphMetricsCalculator.ComputeEdgeBetweenness(builder.Graph, paths).ToDictionary(x => x.OwnerId);
        Assert.That(edgeBetweenness["edge-from-A-to-B"].Absolute, Is.EqualTo(3.0)); // A->B, A->C, C->B
        Assert.That(edgeBetweenness["edge-from-A-to-B"].Normalized, Is.EqualTo(3.0 / 2.0));
        Assert.That(edgeBetweenness["edge-from-B-to-C"].Absolute, Is.EqualTo(3.0)); // A->C, B->C, B->A
        Assert.That(edgeBetweenness["edge-from-B-to-C"].Normalized, Is.EqualTo(3.0 / 2.0));
        Assert.That(edgeBetweenness["edge-from-C-to-A"].Absolute, Is.EqualTo(3.0)); // B->A, C->A, C->B
        Assert.That(edgeBetweenness["edge-from-C-to-A"].Normalized, Is.EqualTo(3.0 / 2.0));
    }

    [Test]
    public void LinearGraph_Metrics()
    {
        var builder = new RelaxedGraphBuilder();
        builder.TryAddEdge("A", "B");
        builder.TryAddEdge("B", "C");
        builder.TryAddEdge("C", "D");

        var paths = ShortestPathsFinder.FindAllShortestPaths(builder.Graph);

        Assert.That(GraphMetricsCalculator.ComputeGraphDensity(builder.Graph), Is.EqualTo(0.25));
        Assert.That(GraphMetricsCalculator.ComputeDiameter(paths), Is.EqualTo(3)); // A -> D
        var avg = (1 + 2 + 3 + 1 + 2 + 1) / 12.0; // A->B, A->C, A->D, B->C, B->D, C->D
        Assert.That(GraphMetricsCalculator.ComputeAveragePathLength(builder.Graph, paths), Is.EqualTo(avg));

        var betweenness = GraphMetricsCalculator.ComputeBetweennessCentrality(builder.Graph, paths).ToDictionary(x => x.OwnerId);
        Assert.That(betweenness["A"].Absolute, Is.EqualTo(0.0));
        Assert.That(betweenness["A"].Normalized, Is.EqualTo(0.0));
        Assert.That(betweenness["B"].Absolute, Is.EqualTo(2.0)); // A->C, A->D
        Assert.That(betweenness["B"].Normalized, Is.EqualTo(2.0 / 6.0)); // A->C, A->D
        Assert.That(betweenness["C"].Absolute, Is.EqualTo(2.0)); // A->D, B->D
        Assert.That(betweenness["C"].Normalized, Is.EqualTo(2.0 / 6.0)); // A->D, B->D
        Assert.That(betweenness["D"].Absolute, Is.EqualTo(0.0));
        Assert.That(betweenness["D"].Normalized, Is.EqualTo(0.0));

        var edgeBetweenness = GraphMetricsCalculator.ComputeEdgeBetweenness(builder.Graph, paths).ToDictionary(x => x.OwnerId);
        Assert.That(edgeBetweenness["edge-from-A-to-B"].Absolute, Is.EqualTo(3.0)); // A->B, A->C, A->D
        Assert.That(edgeBetweenness["edge-from-A-to-B"].Normalized, Is.EqualTo(3.0 / 6.0));
        Assert.That(edgeBetweenness["edge-from-B-to-C"].Absolute, Is.EqualTo(4.0)); // A->C, A->D, B->C, B->D
        Assert.That(edgeBetweenness["edge-from-B-to-C"].Normalized, Is.EqualTo(4.0 / 6.0));
        Assert.That(edgeBetweenness["edge-from-C-to-D"].Absolute, Is.EqualTo(3.0)); // A->D, B->D, C->D
        Assert.That(edgeBetweenness["edge-from-C-to-D"].Normalized, Is.EqualTo(3.0 / 6.0));
    }

    [Test]
    public void DisconnectedGraph_Metrics()
    {
        var builder = new RelaxedGraphBuilder();
        builder.TryAddEdge("A", "B"); // Component 1
        builder.TryAddEdge("C", "D"); // Component 2
        var graph = builder.Graph;
        var paths = ShortestPathsFinder.FindAllShortestPaths(graph);

        Assert.That(GraphMetricsCalculator.ComputeGraphDensity(graph), Is.EqualTo(0.1667).Within(0.0001)); // 2 / (4 * 3)
        Assert.That(GraphMetricsCalculator.ComputeDiameter(paths), Is.EqualTo(1)); // Max within components
        Assert.That(GraphMetricsCalculator.ComputeAveragePathLength(graph, paths), Is.EqualTo(2.0 / 12.0)); // 2 paths / 12 pairs

        var betweenness = GraphMetricsCalculator.ComputeBetweennessCentrality(graph, paths).ToDictionary(x => x.OwnerId);
        Assert.That(betweenness["A"].Absolute, Is.EqualTo(0.0));
        Assert.That(betweenness["A"].Normalized, Is.EqualTo(0.0));
        Assert.That(betweenness["B"].Absolute, Is.EqualTo(0.0));
        Assert.That(betweenness["B"].Normalized, Is.EqualTo(0.0));
        Assert.That(betweenness["C"].Absolute, Is.EqualTo(0.0));
        Assert.That(betweenness["C"].Normalized, Is.EqualTo(0.0));
        Assert.That(betweenness["D"].Absolute, Is.EqualTo(0.0));
        Assert.That(betweenness["D"].Normalized, Is.EqualTo(0.0));

        var edgeBetweenness = GraphMetricsCalculator.ComputeEdgeBetweenness(graph, paths).ToDictionary(x => x.OwnerId);
        Assert.That(edgeBetweenness["edge-from-A-to-B"].Absolute, Is.EqualTo(1.0));
        Assert.That(edgeBetweenness["edge-from-A-to-B"].Normalized, Is.EqualTo(1.0 / 6.0));
        Assert.That(edgeBetweenness["edge-from-C-to-D"].Absolute, Is.EqualTo(1.0));
        Assert.That(edgeBetweenness["edge-from-C-to-D"].Normalized, Is.EqualTo(1.0 / 6.0));
    }

    [Test]
    public void DiamondGraph_Metrics()
    {
        var builder = new RelaxedGraphBuilder();
        builder.TryAddEdge("A", "B");
        builder.TryAddEdge("A", "C");
        builder.TryAddEdge("B", "D");
        builder.TryAddEdge("C", "D");
        var graph = builder.Graph;
        var paths = ShortestPathsFinder.FindAllShortestPaths(graph);

        Assert.That(GraphMetricsCalculator.ComputeGraphDensity(graph), Is.EqualTo(0.3333).Within(0.0001)); // 4 / (4 * 3)
        Assert.That(GraphMetricsCalculator.ComputeDiameter(paths), Is.EqualTo(2)); // A -> D
        Assert.That(GraphMetricsCalculator.ComputeAveragePathLength(graph, paths), Is.EqualTo(6.0 / 12.0)); // 6 hops / 12 pairs

        var betweenness = GraphMetricsCalculator.ComputeBetweennessCentrality(graph, paths).ToDictionary(x => x.OwnerId);
        Assert.That(betweenness["A"].Absolute, Is.EqualTo(0.0));
        Assert.That(betweenness["A"].Normalized, Is.EqualTo(0.0));
        Assert.That(betweenness["B"].Absolute, Is.EqualTo(0.5)); // A->D (1/2)
        Assert.That(betweenness["B"].Normalized, Is.EqualTo(0.5 / 6.0));
        Assert.That(betweenness["C"].Absolute, Is.EqualTo(0.5)); // A->D (1/2)
        Assert.That(betweenness["C"].Normalized, Is.EqualTo(0.5 / 6.0));
        Assert.That(betweenness["D"].Absolute, Is.EqualTo(0.0));
        Assert.That(betweenness["D"].Normalized, Is.EqualTo(0.0));

        var edgeBetweenness = GraphMetricsCalculator.ComputeEdgeBetweenness(graph, paths).ToDictionary(x => x.OwnerId);
        Assert.That(edgeBetweenness["edge-from-A-to-B"].Absolute, Is.EqualTo(1.5)); // A->B, A->D (1/2)
        Assert.That(edgeBetweenness["edge-from-A-to-B"].Normalized, Is.EqualTo(1.5 / 6.0));
        Assert.That(edgeBetweenness["edge-from-A-to-C"].Absolute, Is.EqualTo(1.5)); // A->C, A->D (1/2)
        Assert.That(edgeBetweenness["edge-from-A-to-C"].Normalized, Is.EqualTo(1.5 / 6.0));
        Assert.That(edgeBetweenness["edge-from-B-to-D"].Absolute, Is.EqualTo(1.5)); // B->D, A->D (1/2)
        Assert.That(edgeBetweenness["edge-from-B-to-D"].Normalized, Is.EqualTo(1.5 / 6.0));
        Assert.That(edgeBetweenness["edge-from-C-to-D"].Absolute, Is.EqualTo(1.5)); // C->D, A->D (1/2)
        Assert.That(edgeBetweenness["edge-from-C-to-D"].Normalized, Is.EqualTo(1.5 / 6.0));
    }
}