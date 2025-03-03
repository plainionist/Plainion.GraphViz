﻿using System.Linq;
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
        Assert.That(GraphMetricsCalculator.ComputeAveragePathLength(paths), Is.EqualTo(0.0));
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
        Assert.That(GraphMetricsCalculator.ComputeAveragePathLength(paths), Is.EqualTo(1.0)); // Only 1 path
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
        Assert.That(GraphMetricsCalculator.ComputeAveragePathLength(paths), Is.EqualTo(1.5)); // 9 hops / 6 paths
        var betweenness = GraphMetricsCalculator.ComputeBetweennessCentrality(builder.Graph, paths);
        Assert.That(betweenness["A"], Is.EqualTo(0.5)); // A on B -> C
        Assert.That(betweenness["B"], Is.EqualTo(0.5)); // B on C -> A
        Assert.That(betweenness["C"], Is.EqualTo(0.5)); // C on A -> B
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
        var avg = (1 + 2 + 3 + 1 + 2 + 1) / 6.0; // A->B, A->C, A->D, B->C, B->D, C->D
        Assert.That(GraphMetricsCalculator.ComputeAveragePathLength(paths), Is.EqualTo(avg)); // 1.666...
        var betweenness = GraphMetricsCalculator.ComputeBetweennessCentrality(builder.Graph, paths);
        Assert.That(betweenness["A"], Is.EqualTo(0.0)); // Endpoint
        Assert.That(betweenness["B"], Is.EqualTo(2.0 / 6.0)); // On A->C, A->D
        Assert.That(betweenness["C"], Is.EqualTo(2.0 / 6.0)); // On A->D, B->D
        Assert.That(betweenness["D"], Is.EqualTo(0.0)); // Endpoint
    }
}