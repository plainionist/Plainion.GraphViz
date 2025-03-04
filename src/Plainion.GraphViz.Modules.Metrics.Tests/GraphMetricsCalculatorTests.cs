using System.Linq;
using NUnit.Framework;
using Plainion.Graphs;
using Plainion.GraphViz.Modules.Metrics.Algorithms;

namespace Plainion.GraphViz.Modules.Metrics.Tests;

[TestFixture]
public class GraphMetricsCalculatorTests
{
    [Test]
    public void EmptyGraph()
    {
        var builder = new RelaxedGraphBuilder();

        var paths = ShortestPathsFinder.FindAllShortestPaths(builder.Graph);

        Assert.That(GraphMetricsCalculator.ComputeGraphDensity(builder.Graph), Is.EqualTo(0));
        Assert.That(GraphMetricsCalculator.ComputeDiameter(paths), Is.EqualTo(0));
        Assert.That(GraphMetricsCalculator.ComputeAveragePathLength(builder.Graph, paths), Is.EqualTo(0.0));

        var betweenness = GraphMetricsCalculator.ComputeBetweennessCentrality(builder.Graph, paths);
        Assert.That(betweenness, Is.Empty);

        var edgeBetweenness = GraphMetricsCalculator.ComputeEdgeBetweenness(builder.Graph, paths);
        Assert.That(edgeBetweenness, Is.Empty);

        var closeness = GraphMetricsCalculator.ComputeClosenessCentrality(builder.Graph, paths).ToDictionary(x => x.Owner.Id);
        Assert.That(closeness, Is.Empty);
    }

    [Test]
    public void SingleNodeGraph()
    {
        var builder = new RelaxedGraphBuilder();
        builder.TryAddNode("A"); 

        var paths = ShortestPathsFinder.FindAllShortestPaths(builder.Graph);

        Assert.That(GraphMetricsCalculator.ComputeGraphDensity(builder.Graph), Is.EqualTo(0)); // 0 / (1 * 0) = 0
        Assert.That(GraphMetricsCalculator.ComputeDiameter(paths), Is.EqualTo(0)); // No paths
        Assert.That(GraphMetricsCalculator.ComputeAveragePathLength(builder.Graph, paths), Is.EqualTo(0.0)); // No pairs

        var betweenness = GraphMetricsCalculator.ComputeBetweennessCentrality(builder.Graph, paths).ToDictionary(x => x.Owner.Id);
        Assert.That(betweenness["A"].Absolute, Is.EqualTo(0.0)); // No paths
        Assert.That(betweenness["A"].Normalized, Is.EqualTo(0.0));

        var edgeBetweenness = GraphMetricsCalculator.ComputeEdgeBetweenness(builder.Graph, paths).ToDictionary(x => x.Owner.Id, x => x.Absolute);
        Assert.That(edgeBetweenness.Count, Is.EqualTo(0)); // No edges

        var closeness = GraphMetricsCalculator.ComputeClosenessCentrality(builder.Graph, paths).ToDictionary(x => x.Owner.Id);
        Assert.That(closeness["A"].Absolute, Is.EqualTo(0.0)); // 1/0 → 0
        Assert.That(closeness["A"].Normalized, Is.EqualTo(0.0));
    }

    [Test]
    public void SingleEdgeGraph()
    {
        var builder = new RelaxedGraphBuilder();
        builder.TryAddEdge("A", "B");

        var paths = ShortestPathsFinder.FindAllShortestPaths(builder.Graph);

        Assert.That(GraphMetricsCalculator.ComputeGraphDensity(builder.Graph), Is.EqualTo(0.5));
        Assert.That(GraphMetricsCalculator.ComputeDiameter(paths), Is.EqualTo(1)); // A -> B
        Assert.That(GraphMetricsCalculator.ComputeAveragePathLength(builder.Graph, paths), Is.EqualTo(1.0 / 2.0)); // Only 1 path

        var betweenness = GraphMetricsCalculator.ComputeBetweennessCentrality(builder.Graph, paths).ToDictionary(x => x.Owner.Id);
        Assert.That(betweenness["A"].Absolute, Is.EqualTo(0.0)); // No intermediate nodes
        Assert.That(betweenness["B"].Absolute, Is.EqualTo(0.0));

        var edgeBetweenness = GraphMetricsCalculator.ComputeEdgeBetweenness(builder.Graph, paths).ToDictionary(x => x.Owner.Id);
        Assert.That(edgeBetweenness["edge-from-A-to-B"].Absolute, Is.EqualTo(1.0)); // A -> B path
        Assert.That(edgeBetweenness["edge-from-A-to-B"].Normalized, Is.EqualTo(0.0)); // maxPairs = 0, so normalized = 0

        var closeness = GraphMetricsCalculator.ComputeClosenessCentrality(builder.Graph, paths).ToDictionary(x => x.Owner.Id);
        Assert.That(closeness["A"].Absolute, Is.EqualTo(1.0 / 1.0)); // 1/1
        Assert.That(closeness["A"].Normalized, Is.EqualTo(1.0 / 1.0)); // (2-1)/1
        Assert.That(closeness["B"].Absolute, Is.EqualTo(0.0)); // 1/0 → 0
        Assert.That(closeness["B"].Normalized, Is.EqualTo(0.0));
    }

    [Test]
    public void TriangleGraph()
    {
        var builder = new RelaxedGraphBuilder();
        builder.TryAddEdge("A", "B");
        builder.TryAddEdge("B", "C");
        builder.TryAddEdge("C", "A");

        var paths = ShortestPathsFinder.FindAllShortestPaths(builder.Graph);

        Assert.That(GraphMetricsCalculator.ComputeGraphDensity(builder.Graph), Is.EqualTo(0.5));
        Assert.That(GraphMetricsCalculator.ComputeDiameter(paths), Is.EqualTo(2)); // e.g., A -> B -> C
        Assert.That(GraphMetricsCalculator.ComputeAveragePathLength(builder.Graph, paths), Is.EqualTo(1.5)); // 9 hops / 6 paths

        var betweenness = GraphMetricsCalculator.ComputeBetweennessCentrality(builder.Graph, paths).ToDictionary(x => x.Owner.Id);
        Assert.That(betweenness["A"].Absolute, Is.EqualTo(1.0)); // C -> B
        Assert.That(betweenness["A"].Normalized, Is.EqualTo(1.0 / 2.0)); // C -> B
        Assert.That(betweenness["B"].Absolute, Is.EqualTo(1.0)); // A -> C
        Assert.That(betweenness["B"].Normalized, Is.EqualTo(1.0 / 2.0)); // A -> C
        Assert.That(betweenness["C"].Absolute, Is.EqualTo(1.0)); // B -> A
        Assert.That(betweenness["C"].Normalized, Is.EqualTo(1.0 / 2.0)); // B -> A

        var edgeBetweenness = GraphMetricsCalculator.ComputeEdgeBetweenness(builder.Graph, paths).ToDictionary(x => x.Owner.Id);
        Assert.That(edgeBetweenness["edge-from-A-to-B"].Absolute, Is.EqualTo(3.0)); // A->B, A->C, C->B
        Assert.That(edgeBetweenness["edge-from-A-to-B"].Normalized, Is.EqualTo(3.0 / 2.0));
        Assert.That(edgeBetweenness["edge-from-B-to-C"].Absolute, Is.EqualTo(3.0)); // A->C, B->C, B->A
        Assert.That(edgeBetweenness["edge-from-B-to-C"].Normalized, Is.EqualTo(3.0 / 2.0));
        Assert.That(edgeBetweenness["edge-from-C-to-A"].Absolute, Is.EqualTo(3.0)); // B->A, C->A, C->B
        Assert.That(edgeBetweenness["edge-from-C-to-A"].Normalized, Is.EqualTo(3.0 / 2.0));

        var closeness = GraphMetricsCalculator.ComputeClosenessCentrality(builder.Graph, paths).ToDictionary(x => x.Owner.Id);
        Assert.That(closeness["A"].Absolute, Is.EqualTo(1.0 / 3.0)); // 1/(1+2)
        Assert.That(closeness["A"].Normalized, Is.EqualTo(2.0 / 3.0)); // (3-1)/3
        Assert.That(closeness["B"].Absolute, Is.EqualTo(1.0 / 3.0));
        Assert.That(closeness["B"].Normalized, Is.EqualTo(2.0 / 3.0));
        Assert.That(closeness["C"].Absolute, Is.EqualTo(1.0 / 3.0));
        Assert.That(closeness["C"].Normalized, Is.EqualTo(2.0 / 3.0));
    }

    [Test]
    public void LinearGraph()
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

        var betweenness = GraphMetricsCalculator.ComputeBetweennessCentrality(builder.Graph, paths).ToDictionary(x => x.Owner.Id);
        Assert.That(betweenness["A"].Absolute, Is.EqualTo(0.0));
        Assert.That(betweenness["A"].Normalized, Is.EqualTo(0.0));
        Assert.That(betweenness["B"].Absolute, Is.EqualTo(2.0)); // A->C, A->D
        Assert.That(betweenness["B"].Normalized, Is.EqualTo(2.0 / 6.0)); // A->C, A->D
        Assert.That(betweenness["C"].Absolute, Is.EqualTo(2.0)); // A->D, B->D
        Assert.That(betweenness["C"].Normalized, Is.EqualTo(2.0 / 6.0)); // A->D, B->D
        Assert.That(betweenness["D"].Absolute, Is.EqualTo(0.0));
        Assert.That(betweenness["D"].Normalized, Is.EqualTo(0.0));

        var edgeBetweenness = GraphMetricsCalculator.ComputeEdgeBetweenness(builder.Graph, paths).ToDictionary(x => x.Owner.Id);
        Assert.That(edgeBetweenness["edge-from-A-to-B"].Absolute, Is.EqualTo(3.0)); // A->B, A->C, A->D
        Assert.That(edgeBetweenness["edge-from-A-to-B"].Normalized, Is.EqualTo(3.0 / 6.0));
        Assert.That(edgeBetweenness["edge-from-B-to-C"].Absolute, Is.EqualTo(4.0)); // A->C, A->D, B->C, B->D
        Assert.That(edgeBetweenness["edge-from-B-to-C"].Normalized, Is.EqualTo(4.0 / 6.0));
        Assert.That(edgeBetweenness["edge-from-C-to-D"].Absolute, Is.EqualTo(3.0)); // A->D, B->D, C->D
        Assert.That(edgeBetweenness["edge-from-C-to-D"].Normalized, Is.EqualTo(3.0 / 6.0));

        var closeness = GraphMetricsCalculator.ComputeClosenessCentrality(builder.Graph, paths).ToDictionary(x => x.Owner.Id);
        Assert.That(closeness["A"].Absolute, Is.EqualTo(1.0 / 6.0)); // 1/(1+2+3)
        Assert.That(closeness["A"].Normalized, Is.EqualTo(3.0 / 6.0)); // (4-1)/6
        Assert.That(closeness["B"].Absolute, Is.EqualTo(1.0 / 3.0)); // 1/(1+2)
        Assert.That(closeness["B"].Normalized, Is.EqualTo(3.0 / 3.0)); // (4-1)/3
        Assert.That(closeness["C"].Absolute, Is.EqualTo(1.0 / 1.0)); // 1/1
        Assert.That(closeness["C"].Normalized, Is.EqualTo(3.0 / 1.0)); // (4-1)/1
        Assert.That(closeness["D"].Absolute, Is.EqualTo(0.0)); // 1/0 → 0
        Assert.That(closeness["D"].Normalized, Is.EqualTo(0.0));
    }

    [Test]
    public void DisconnectedGraph()
    {
        var builder = new RelaxedGraphBuilder();
        builder.TryAddEdge("A", "B"); // Component 1
        builder.TryAddEdge("C", "D"); // Component 2

        var paths = ShortestPathsFinder.FindAllShortestPaths(builder.Graph);

        Assert.That(GraphMetricsCalculator.ComputeGraphDensity(builder.Graph), Is.EqualTo(0.1667).Within(0.0001)); // 2 / (4 * 3)
        Assert.That(GraphMetricsCalculator.ComputeDiameter(paths), Is.EqualTo(1)); // Max within components
        Assert.That(GraphMetricsCalculator.ComputeAveragePathLength(builder.Graph, paths), Is.EqualTo(2.0 / 12.0)); // 2 paths / 12 pairs

        var betweenness = GraphMetricsCalculator.ComputeBetweennessCentrality(builder.Graph, paths).ToDictionary(x => x.Owner.Id);
        Assert.That(betweenness["A"].Absolute, Is.EqualTo(0.0));
        Assert.That(betweenness["A"].Normalized, Is.EqualTo(0.0));
        Assert.That(betweenness["B"].Absolute, Is.EqualTo(0.0));
        Assert.That(betweenness["B"].Normalized, Is.EqualTo(0.0));
        Assert.That(betweenness["C"].Absolute, Is.EqualTo(0.0));
        Assert.That(betweenness["C"].Normalized, Is.EqualTo(0.0));
        Assert.That(betweenness["D"].Absolute, Is.EqualTo(0.0));
        Assert.That(betweenness["D"].Normalized, Is.EqualTo(0.0));

        var edgeBetweenness = GraphMetricsCalculator.ComputeEdgeBetweenness(builder.Graph, paths).ToDictionary(x => x.Owner.Id);
        Assert.That(edgeBetweenness["edge-from-A-to-B"].Absolute, Is.EqualTo(1.0));
        Assert.That(edgeBetweenness["edge-from-A-to-B"].Normalized, Is.EqualTo(1.0 / 6.0));
        Assert.That(edgeBetweenness["edge-from-C-to-D"].Absolute, Is.EqualTo(1.0));
        Assert.That(edgeBetweenness["edge-from-C-to-D"].Normalized, Is.EqualTo(1.0 / 6.0));

        var closeness = GraphMetricsCalculator.ComputeClosenessCentrality(builder.Graph, paths).ToDictionary(x => x.Owner.Id);
        Assert.That(closeness["A"].Absolute, Is.EqualTo(1.0 / 1.0)); // 1/1
        Assert.That(closeness["A"].Normalized, Is.EqualTo(3.0 / 1.0)); // (4-1)/1
        Assert.That(closeness["B"].Absolute, Is.EqualTo(0.0)); // 1/0 → 0
        Assert.That(closeness["B"].Normalized, Is.EqualTo(0.0));
        Assert.That(closeness["C"].Absolute, Is.EqualTo(1.0 / 1.0)); // 1/1
        Assert.That(closeness["C"].Normalized, Is.EqualTo(3.0 / 1.0));
        Assert.That(closeness["D"].Absolute, Is.EqualTo(0.0)); // 1/0 → 0
        Assert.That(closeness["D"].Normalized, Is.EqualTo(0.0));
    }

    [Test]
    public void DiamondGraph()
    {
        var builder = new RelaxedGraphBuilder();
        builder.TryAddEdge("A", "B");
        builder.TryAddEdge("A", "C");
        builder.TryAddEdge("B", "D");
        builder.TryAddEdge("C", "D");

        var paths = ShortestPathsFinder.FindAllShortestPaths(builder.Graph);

        Assert.That(GraphMetricsCalculator.ComputeGraphDensity(builder.Graph), Is.EqualTo(0.3333).Within(0.0001)); // 4 / (4 * 3)
        Assert.That(GraphMetricsCalculator.ComputeDiameter(paths), Is.EqualTo(2)); // A -> D
        Assert.That(GraphMetricsCalculator.ComputeAveragePathLength(builder.Graph, paths), Is.EqualTo(6.0 / 12.0)); // 6 hops / 12 pairs

        var betweenness = GraphMetricsCalculator.ComputeBetweennessCentrality(builder.Graph, paths).ToDictionary(x => x.Owner.Id);
        Assert.That(betweenness["A"].Absolute, Is.EqualTo(0.0));
        Assert.That(betweenness["A"].Normalized, Is.EqualTo(0.0));
        Assert.That(betweenness["B"].Absolute, Is.EqualTo(0.5)); // A->D (1/2)
        Assert.That(betweenness["B"].Normalized, Is.EqualTo(0.5 / 6.0));
        Assert.That(betweenness["C"].Absolute, Is.EqualTo(0.5)); // A->D (1/2)
        Assert.That(betweenness["C"].Normalized, Is.EqualTo(0.5 / 6.0));
        Assert.That(betweenness["D"].Absolute, Is.EqualTo(0.0));
        Assert.That(betweenness["D"].Normalized, Is.EqualTo(0.0));

        var edgeBetweenness = GraphMetricsCalculator.ComputeEdgeBetweenness(builder.Graph, paths).ToDictionary(x => x.Owner.Id);
        Assert.That(edgeBetweenness["edge-from-A-to-B"].Absolute, Is.EqualTo(1.5)); // A->B, A->D (1/2)
        Assert.That(edgeBetweenness["edge-from-A-to-B"].Normalized, Is.EqualTo(1.5 / 6.0));
        Assert.That(edgeBetweenness["edge-from-A-to-C"].Absolute, Is.EqualTo(1.5)); // A->C, A->D (1/2)
        Assert.That(edgeBetweenness["edge-from-A-to-C"].Normalized, Is.EqualTo(1.5 / 6.0));
        Assert.That(edgeBetweenness["edge-from-B-to-D"].Absolute, Is.EqualTo(1.5)); // B->D, A->D (1/2)
        Assert.That(edgeBetweenness["edge-from-B-to-D"].Normalized, Is.EqualTo(1.5 / 6.0));
        Assert.That(edgeBetweenness["edge-from-C-to-D"].Absolute, Is.EqualTo(1.5)); // C->D, A->D (1/2)
        Assert.That(edgeBetweenness["edge-from-C-to-D"].Normalized, Is.EqualTo(1.5 / 6.0));
        
        var closeness = GraphMetricsCalculator.ComputeClosenessCentrality(builder.Graph, paths).ToDictionary(x => x.Owner.Id);
        Assert.That(closeness["A"].Absolute, Is.EqualTo(1.0 / 4.0)); // 1/(1+1+2)
        Assert.That(closeness["A"].Normalized, Is.EqualTo(3.0 / 4.0)); // (4-1)/4
        Assert.That(closeness["B"].Absolute, Is.EqualTo(1.0 / 1.0)); // 1/1
        Assert.That(closeness["B"].Normalized, Is.EqualTo(3.0 / 1.0)); // (4-1)/1
        Assert.That(closeness["C"].Absolute, Is.EqualTo(1.0 / 1.0)); // 1/1
        Assert.That(closeness["C"].Normalized, Is.EqualTo(3.0 / 1.0));
        Assert.That(closeness["D"].Absolute, Is.EqualTo(0.0)); // 1/0 -> 0
        Assert.That(closeness["D"].Normalized, Is.EqualTo(0.0));
    }

    [Test]
    public void StarGraph()
    {
        var builder = new RelaxedGraphBuilder();
        builder.TryAddEdge("A", "B");
        builder.TryAddEdge("A", "C");
        builder.TryAddEdge("A", "D");
        builder.TryAddEdge("A", "E");

        var paths = ShortestPathsFinder.FindAllShortestPaths(builder.Graph);

        Assert.That(GraphMetricsCalculator.ComputeGraphDensity(builder.Graph), Is.EqualTo(0.2).Within(0.0001)); // 4/(5*4)
        Assert.That(GraphMetricsCalculator.ComputeDiameter(paths), Is.EqualTo(1));
        Assert.That(GraphMetricsCalculator.ComputeAveragePathLength(builder.Graph, paths), Is.EqualTo(4.0 / 20.0)); // 4/20

        var betweenness = GraphMetricsCalculator.ComputeBetweennessCentrality(builder.Graph, paths).ToDictionary(x => x.Owner.Id);
        Assert.That(betweenness["A"].Absolute, Is.EqualTo(0.0)); // Not intermediate
        Assert.That(betweenness["A"].Normalized, Is.EqualTo(0.0));
        Assert.That(betweenness["B"].Absolute, Is.EqualTo(0.0));
        Assert.That(betweenness["B"].Normalized, Is.EqualTo(0.0));
        Assert.That(betweenness["C"].Absolute, Is.EqualTo(0.0));
        Assert.That(betweenness["C"].Normalized, Is.EqualTo(0.0));
        Assert.That(betweenness["D"].Absolute, Is.EqualTo(0.0));
        Assert.That(betweenness["D"].Normalized, Is.EqualTo(0.0));
        Assert.That(betweenness["E"].Absolute, Is.EqualTo(0.0));
        Assert.That(betweenness["E"].Normalized, Is.EqualTo(0.0));

        var edgeBetweenness = GraphMetricsCalculator.ComputeEdgeBetweenness(builder.Graph, paths).ToDictionary(x => x.Owner.Id);
        Assert.That(edgeBetweenness["edge-from-A-to-B"].Absolute, Is.EqualTo(1.0)); // A->B path
        Assert.That(edgeBetweenness["edge-from-A-to-B"].Normalized, Is.EqualTo(1.0 / 12.0)); // (5-1)(5-2)
        Assert.That(edgeBetweenness["edge-from-A-to-C"].Absolute, Is.EqualTo(1.0));
        Assert.That(edgeBetweenness["edge-from-A-to-C"].Normalized, Is.EqualTo(1.0 / 12.0));
        Assert.That(edgeBetweenness["edge-from-A-to-D"].Absolute, Is.EqualTo(1.0));
        Assert.That(edgeBetweenness["edge-from-A-to-D"].Normalized, Is.EqualTo(1.0 / 12.0));
        Assert.That(edgeBetweenness["edge-from-A-to-E"].Absolute, Is.EqualTo(1.0));
        Assert.That(edgeBetweenness["edge-from-A-to-E"].Normalized, Is.EqualTo(1.0 / 12.0));

        var closeness = GraphMetricsCalculator.ComputeClosenessCentrality(builder.Graph, paths).ToDictionary(x => x.Owner.Id);
        Assert.That(closeness["A"].Absolute, Is.EqualTo(0.25)); // 1/(1+1+1+1)
        Assert.That(closeness["A"].Normalized, Is.EqualTo(1.0)); // (5-1)/4
        Assert.That(closeness["B"].Absolute, Is.EqualTo(0.0)); // 1/0 → 0
        Assert.That(closeness["B"].Normalized, Is.EqualTo(0.0));
        Assert.That(closeness["C"].Absolute, Is.EqualTo(0.0));
        Assert.That(closeness["C"].Normalized, Is.EqualTo(0.0));
        Assert.That(closeness["D"].Absolute, Is.EqualTo(0.0));
        Assert.That(closeness["D"].Normalized, Is.EqualTo(0.0));
        Assert.That(closeness["E"].Absolute, Is.EqualTo(0.0));
        Assert.That(closeness["E"].Normalized, Is.EqualTo(0.0));
    }

    [Test]
    public void ReverseStarGraph()
    {
        var builder = new RelaxedGraphBuilder();
        builder.TryAddEdge("B", "A");
        builder.TryAddEdge("C", "A");
        builder.TryAddEdge("D", "A");
        builder.TryAddEdge("E", "A");

        var paths = ShortestPathsFinder.FindAllShortestPaths(builder.Graph);

        Assert.That(GraphMetricsCalculator.ComputeGraphDensity(builder.Graph), Is.EqualTo(0.2).Within(0.0001)); // 4/(5*4)
        Assert.That(GraphMetricsCalculator.ComputeDiameter(paths), Is.EqualTo(1));
        Assert.That(GraphMetricsCalculator.ComputeAveragePathLength(builder.Graph, paths), Is.EqualTo(4.0 / 20.0)); // 4/20

        var betweenness = GraphMetricsCalculator.ComputeBetweennessCentrality(builder.Graph, paths).ToDictionary(x => x.Owner.Id);
        Assert.That(betweenness["A"].Absolute, Is.EqualTo(0.0)); // Not intermediate
        Assert.That(betweenness["A"].Normalized, Is.EqualTo(0.0));
        Assert.That(betweenness["B"].Absolute, Is.EqualTo(0.0));
        Assert.That(betweenness["B"].Normalized, Is.EqualTo(0.0));
        Assert.That(betweenness["C"].Absolute, Is.EqualTo(0.0));
        Assert.That(betweenness["C"].Normalized, Is.EqualTo(0.0));
        Assert.That(betweenness["D"].Absolute, Is.EqualTo(0.0));
        Assert.That(betweenness["D"].Normalized, Is.EqualTo(0.0));
        Assert.That(betweenness["E"].Absolute, Is.EqualTo(0.0));
        Assert.That(betweenness["E"].Normalized, Is.EqualTo(0.0));

        var edgeBetweenness = GraphMetricsCalculator.ComputeEdgeBetweenness(builder.Graph, paths).ToDictionary(x => x.Owner.Id);
        Assert.That(edgeBetweenness["edge-from-B-to-A"].Absolute, Is.EqualTo(1.0)); // B->A path
        Assert.That(edgeBetweenness["edge-from-B-to-A"].Normalized, Is.EqualTo(1.0 / 12.0)); // (5-1)(5-2)
        Assert.That(edgeBetweenness["edge-from-C-to-A"].Absolute, Is.EqualTo(1.0));
        Assert.That(edgeBetweenness["edge-from-C-to-A"].Normalized, Is.EqualTo(1.0 / 12.0));
        Assert.That(edgeBetweenness["edge-from-D-to-A"].Absolute, Is.EqualTo(1.0));
        Assert.That(edgeBetweenness["edge-from-D-to-A"].Normalized, Is.EqualTo(1.0 / 12.0));
        Assert.That(edgeBetweenness["edge-from-E-to-A"].Absolute, Is.EqualTo(1.0));
        Assert.That(edgeBetweenness["edge-from-E-to-A"].Normalized, Is.EqualTo(1.0 / 12.0));

        var closeness = GraphMetricsCalculator.ComputeClosenessCentrality(builder.Graph, paths).ToDictionary(x => x.Owner.Id);
        Assert.That(closeness["A"].Absolute, Is.EqualTo(0.0)); // 1/0 → 0 (no outgoing)
        Assert.That(closeness["A"].Normalized, Is.EqualTo(0.0));
        Assert.That(closeness["B"].Absolute, Is.EqualTo(1.0)); // 1/1 (B->A)
        Assert.That(closeness["B"].Normalized, Is.EqualTo(4.0)); // (5-1)/1
        Assert.That(closeness["C"].Absolute, Is.EqualTo(1.0)); // 1/1 (C->A)
        Assert.That(closeness["C"].Normalized, Is.EqualTo(4.0));
        Assert.That(closeness["D"].Absolute, Is.EqualTo(1.0)); // 1/1 (D->A)
        Assert.That(closeness["D"].Normalized, Is.EqualTo(4.0));
        Assert.That(closeness["E"].Absolute, Is.EqualTo(1.0)); // 1/1 (E->A)
        Assert.That(closeness["E"].Normalized, Is.EqualTo(4.0));
    }
}
