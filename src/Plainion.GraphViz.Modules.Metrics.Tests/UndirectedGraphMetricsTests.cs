using System.Linq;
using NUnit.Framework;
using Plainion.Graphs.Undirected;
using Plainion.GraphViz.Modules.Metrics.Algorithms;

namespace Plainion.GraphViz.Modules.Metrics.Tests;

//[TestFixture]
//public class UndirectedGraphMetricsCalculatorTests
//{
//    [Test]
//    public void EmptyGraph()
//    {
//        var builder = new RelaxedGraphBuilder();

//        var paths = ShortestPathsFinder.FindAllShortestPaths(builder.Graph);

//        var closeness = UndirectedGraphMetricsCalculator.ComputeClosenessCentrality(builder.Graph, paths).ToDictionary(x => x.Owner.Id);
//        Assert.That(closeness, Is.Empty);
//    }

//    [Test]
//    public void SingleNodeGraph()
//    {
//        var builder = new RelaxedGraphBuilder();
//        builder.TryAddNode("A"); 

//        var paths = ShortestPathsFinder.FindAllShortestPaths(builder.Graph);

//        var closeness = GraphMetricsCalculator.ComputeClosenessCentrality(builder.Graph, paths).ToDictionary(x => x.Owner.Id);
//        Assert.That(closeness["A"].Absolute, Is.EqualTo(0.0)); // 1/0 → 0
//        Assert.That(closeness["A"].Normalized, Is.EqualTo(0.0));
//    }

//    [Test]
//    public void SingleEdgeGraph()
//    {
//        var builder = new RelaxedGraphBuilder();
//        builder.TryAddEdge("A", "B");

//        var paths = ShortestPathsFinder.FindAllShortestPaths(builder.Graph);

//        var closeness = GraphMetricsCalculator.ComputeClosenessCentrality(builder.Graph, paths).ToDictionary(x => x.Owner.Id);
//        Assert.That(closeness["A"].Absolute, Is.EqualTo(1.0 / 1.0)); // 1/1
//        Assert.That(closeness["A"].Normalized, Is.EqualTo(1.0 / 1.0)); // (2-1)/1
//        Assert.That(closeness["B"].Absolute, Is.EqualTo(0.0)); // 1/0 → 0
//        Assert.That(closeness["B"].Normalized, Is.EqualTo(0.0));
//    }

//    [Test]
//    public void TriangleGraph()
//    {
//        var builder = new RelaxedGraphBuilder();
//        builder.TryAddEdge("A", "B");
//        builder.TryAddEdge("B", "C");
//        builder.TryAddEdge("C", "A");

//        var paths = ShortestPathsFinder.FindAllShortestPaths(builder.Graph);

//        var closeness = GraphMetricsCalculator.ComputeClosenessCentrality(builder.Graph, paths).ToDictionary(x => x.Owner.Id);
//        Assert.That(closeness["A"].Absolute, Is.EqualTo(1.0 / 3.0)); // 1/(1+2)
//        Assert.That(closeness["A"].Normalized, Is.EqualTo(2.0 / 3.0)); // (3-1)/3
//        Assert.That(closeness["B"].Absolute, Is.EqualTo(1.0 / 3.0));
//        Assert.That(closeness["B"].Normalized, Is.EqualTo(2.0 / 3.0));
//        Assert.That(closeness["C"].Absolute, Is.EqualTo(1.0 / 3.0));
//        Assert.That(closeness["C"].Normalized, Is.EqualTo(2.0 / 3.0));
//    }

//    [Test]
//    public void LinearGraph()
//    {
//        var builder = new RelaxedGraphBuilder();
//        builder.TryAddEdge("A", "B");
//        builder.TryAddEdge("B", "C");
//        builder.TryAddEdge("C", "D");

//        var paths = ShortestPathsFinder.FindAllShortestPaths(builder.Graph);

//        var closeness = GraphMetricsCalculator.ComputeClosenessCentrality(builder.Graph, paths).ToDictionary(x => x.Owner.Id);
//        Assert.That(closeness["A"].Absolute, Is.EqualTo(1.0 / 6.0)); // 1/(1+2+3)
//        Assert.That(closeness["A"].Normalized, Is.EqualTo(3.0 / 6.0)); // (4-1)/6
//        Assert.That(closeness["B"].Absolute, Is.EqualTo(1.0 / 3.0)); // 1/(1+2)
//        Assert.That(closeness["B"].Normalized, Is.EqualTo(3.0 / 3.0)); // (4-1)/3
//        Assert.That(closeness["C"].Absolute, Is.EqualTo(1.0 / 1.0)); // 1/1
//        Assert.That(closeness["C"].Normalized, Is.EqualTo(3.0 / 1.0)); // (4-1)/1
//        Assert.That(closeness["D"].Absolute, Is.EqualTo(0.0)); // 1/0 → 0
//        Assert.That(closeness["D"].Normalized, Is.EqualTo(0.0));
//    }

//    [Test]
//    public void DisconnectedGraph()
//    {
//        var builder = new RelaxedGraphBuilder();
//        builder.TryAddEdge("A", "B"); // Component 1
//        builder.TryAddEdge("C", "D"); // Component 2

//        var paths = ShortestPathsFinder.FindAllShortestPaths(builder.Graph);

//        var closeness = GraphMetricsCalculator.ComputeClosenessCentrality(builder.Graph, paths).ToDictionary(x => x.Owner.Id);
//        Assert.That(closeness["A"].Absolute, Is.EqualTo(1.0 / 1.0)); // 1/1
//        Assert.That(closeness["A"].Normalized, Is.EqualTo(3.0 / 1.0)); // (4-1)/1
//        Assert.That(closeness["B"].Absolute, Is.EqualTo(0.0)); // 1/0 → 0
//        Assert.That(closeness["B"].Normalized, Is.EqualTo(0.0));
//        Assert.That(closeness["C"].Absolute, Is.EqualTo(1.0 / 1.0)); // 1/1
//        Assert.That(closeness["C"].Normalized, Is.EqualTo(3.0 / 1.0));
//        Assert.That(closeness["D"].Absolute, Is.EqualTo(0.0)); // 1/0 → 0
//        Assert.That(closeness["D"].Normalized, Is.EqualTo(0.0));
//    }

//    [Test]
//    public void DiamondGraph()
//    {
//        var builder = new RelaxedGraphBuilder();
//        builder.TryAddEdge("A", "B");
//        builder.TryAddEdge("A", "C");
//        builder.TryAddEdge("B", "D");
//        builder.TryAddEdge("C", "D");

//        var paths = ShortestPathsFinder.FindAllShortestPaths(builder.Graph);

//        var closeness = GraphMetricsCalculator.ComputeClosenessCentrality(builder.Graph, paths).ToDictionary(x => x.Owner.Id);
//        Assert.That(closeness["A"].Absolute, Is.EqualTo(1.0 / 4.0)); // 1/(1+1+2)
//        Assert.That(closeness["A"].Normalized, Is.EqualTo(3.0 / 4.0)); // (4-1)/4
//        Assert.That(closeness["B"].Absolute, Is.EqualTo(1.0 / 1.0)); // 1/1
//        Assert.That(closeness["B"].Normalized, Is.EqualTo(3.0 / 1.0)); // (4-1)/1
//        Assert.That(closeness["C"].Absolute, Is.EqualTo(1.0 / 1.0)); // 1/1
//        Assert.That(closeness["C"].Normalized, Is.EqualTo(3.0 / 1.0));
//        Assert.That(closeness["D"].Absolute, Is.EqualTo(0.0)); // 1/0 -> 0
//        Assert.That(closeness["D"].Normalized, Is.EqualTo(0.0));
//    }

//    [Test]
//    public void StarGraph()
//    {
//        var builder = new RelaxedGraphBuilder();
//        builder.TryAddEdge("A", "B");
//        builder.TryAddEdge("A", "C");
//        builder.TryAddEdge("A", "D");
//        builder.TryAddEdge("A", "E");

//        var paths = ShortestPathsFinder.FindAllShortestPaths(builder.Graph);

//        var closeness = GraphMetricsCalculator.ComputeClosenessCentrality(builder.Graph, paths).ToDictionary(x => x.Owner.Id);
//        Assert.That(closeness["A"].Absolute, Is.EqualTo(0.25)); // 1/(1+1+1+1)
//        Assert.That(closeness["A"].Normalized, Is.EqualTo(1.0)); // (5-1)/4
//        Assert.That(closeness["B"].Absolute, Is.EqualTo(0.0)); // 1/0 → 0
//        Assert.That(closeness["B"].Normalized, Is.EqualTo(0.0));
//        Assert.That(closeness["C"].Absolute, Is.EqualTo(0.0));
//        Assert.That(closeness["C"].Normalized, Is.EqualTo(0.0));
//        Assert.That(closeness["D"].Absolute, Is.EqualTo(0.0));
//        Assert.That(closeness["D"].Normalized, Is.EqualTo(0.0));
//        Assert.That(closeness["E"].Absolute, Is.EqualTo(0.0));
//        Assert.That(closeness["E"].Normalized, Is.EqualTo(0.0));
//    }

//    [Test]
//    public void ReverseStarGraph()
//    {
//        var builder = new RelaxedGraphBuilder();
//        builder.TryAddEdge("B", "A");
//        builder.TryAddEdge("C", "A");
//        builder.TryAddEdge("D", "A");
//        builder.TryAddEdge("E", "A");

//        var paths = ShortestPathsFinder.FindAllShortestPaths(builder.Graph);

//        var closeness = GraphMetricsCalculator.ComputeClosenessCentrality(builder.Graph, paths).ToDictionary(x => x.Owner.Id);
//        Assert.That(closeness["A"].Absolute, Is.EqualTo(0.0)); // 1/0 → 0 (no outgoing)
//        Assert.That(closeness["A"].Normalized, Is.EqualTo(0.0));
//        Assert.That(closeness["B"].Absolute, Is.EqualTo(1.0)); // 1/1 (B->A)
//        Assert.That(closeness["B"].Normalized, Is.EqualTo(4.0)); // (5-1)/1
//        Assert.That(closeness["C"].Absolute, Is.EqualTo(1.0)); // 1/1 (C->A)
//        Assert.That(closeness["C"].Normalized, Is.EqualTo(4.0));
//        Assert.That(closeness["D"].Absolute, Is.EqualTo(1.0)); // 1/1 (D->A)
//        Assert.That(closeness["D"].Normalized, Is.EqualTo(4.0));
//        Assert.That(closeness["E"].Absolute, Is.EqualTo(1.0)); // 1/1 (E->A)
//        Assert.That(closeness["E"].Normalized, Is.EqualTo(4.0));
//    }
//}
