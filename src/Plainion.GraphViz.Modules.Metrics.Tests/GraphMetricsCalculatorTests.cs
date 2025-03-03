using System.Linq;
using NUnit.Framework;
using Plainion.Graphs;

namespace Plainion.GraphViz.Modules.Metrics.Tests;

[TestFixture]
public class GraphMetricsCalculatorTests
{
    [Test]
    public void EmptyGraph_ReturnsZeros()
    {
        var builder = new RelaxedGraphBuilder();

        var paths = ShortestPathsFinder.FindAllShortestPaths(builder.Graph);

        Assert.That(GraphMetricsCalculator.ComputeGraphDensity(builder.Graph).Density, Is.EqualTo(0));
        Assert.That(GraphMetricsCalculator.ComputeDiameter(paths), Is.EqualTo(0));
        Assert.That(GraphMetricsCalculator.ComputeAveragePathLength(builder.Graph, paths), Is.EqualTo(0.0));
        var betweenness = GraphMetricsCalculator.ComputeBetweennessCentrality(builder.Graph, paths);
        Assert.That(betweenness.All(kv => kv.Value == 0.0), Is.True);
    }

    [Test]
    public void SingleEdgeGraph_DiameterAndAvgPath()
    {
        var builder = new RelaxedGraphBuilder();
        builder.TryAddEdge("A", "B");

        var paths = ShortestPathsFinder.FindAllShortestPaths(builder.Graph);

        Assert.That(GraphMetricsCalculator.ComputeGraphDensity(builder.Graph).Density, Is.EqualTo(0.5));
        Assert.That(GraphMetricsCalculator.ComputeDiameter(paths), Is.EqualTo(1)); // A -> B
        Assert.That(GraphMetricsCalculator.ComputeAveragePathLength(builder.Graph, paths), Is.EqualTo(1.0 / 2.0)); // Only 1 path
        var betweenness = GraphMetricsCalculator.ComputeBetweennessCentrality(builder.Graph, paths);
        Assert.That(betweenness["A"], Is.EqualTo(0.0)); // No intermediate nodes
        Assert.That(betweenness["B"], Is.EqualTo(0.0));
    }

    [Test]
    public void TriangleGraph_Metrics()
    {
        var builder = new RelaxedGraphBuilder();
        builder.TryAddEdge("A", "B");
        builder.TryAddEdge("B", "C");
        builder.TryAddEdge("C", "A");

        var paths = ShortestPathsFinder.FindAllShortestPaths(builder.Graph);

        Assert.That(GraphMetricsCalculator.ComputeGraphDensity(builder.Graph).Density, Is.EqualTo(0.5));
        Assert.That(GraphMetricsCalculator.ComputeDiameter(paths), Is.EqualTo(2)); // e.g., A -> B -> C
        Assert.That(GraphMetricsCalculator.ComputeAveragePathLength(builder.Graph, paths), Is.EqualTo(1.5)); // 9 hops / 6 paths
        var betweenness = GraphMetricsCalculator.ComputeBetweennessCentrality(builder.Graph, paths);
        Assert.That(betweenness["A"], Is.EqualTo(1.0 / 6.0)); // C -> B
        Assert.That(betweenness["B"], Is.EqualTo(1.0 / 6.0)); // A -> C
        Assert.That(betweenness["C"], Is.EqualTo(1.0 / 6.0)); // B -> A
    }

    [Test]
    public void LinearGraph_Metrics()
    {
        var builder = new RelaxedGraphBuilder();
        builder.TryAddEdge("A", "B");
        builder.TryAddEdge("B", "C");
        builder.TryAddEdge("C", "D");

        var paths = ShortestPathsFinder.FindAllShortestPaths(builder.Graph);

        Assert.That(GraphMetricsCalculator.ComputeGraphDensity(builder.Graph).Density, Is.EqualTo(0.25));
        Assert.That(GraphMetricsCalculator.ComputeDiameter(paths), Is.EqualTo(3)); // A -> D
        var avg = (1 + 2 + 3 + 1 + 2 + 1) / 12.0; // A->B, A->C, A->D, B->C, B->D, C->D
        Assert.That(GraphMetricsCalculator.ComputeAveragePathLength(builder.Graph, paths), Is.EqualTo(avg));
        var betweenness = GraphMetricsCalculator.ComputeBetweennessCentrality(builder.Graph, paths);
        Assert.That(betweenness["A"], Is.EqualTo(0.0));
        Assert.That(betweenness["B"], Is.EqualTo(2.0 / 12.0)); // A -> C, A -> D
        Assert.That(betweenness["C"], Is.EqualTo(2.0 / 12.0)); // A -> D, B -> D
        Assert.That(betweenness["D"], Is.EqualTo(0.0));
    }
}