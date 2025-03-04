using System.Linq;
using NUnit.Framework;
using Plainion.Graphs;
using Plainion.GraphViz.Modules.Metrics.Algorithms;

namespace Plainion.GraphViz.Modules.Metrics.Tests;

[TestFixture]
public class ShortestPathsFinderTests
{
    [Test]
    public void SingleEdge_ReturnsThisEdge()
    {
        var builder = new RelaxedGraphBuilder();
        builder.TryAddEdge("A", "B");

        var result = ShortestPathsFinder.FindAllShortestPaths(builder.Graph);

        var paths = result.Get("A", "B");
        Assert.That(paths.Count, Is.EqualTo(1));
        Assert.That(paths.Single().Count, Is.EqualTo(1));
    }

    [Test]
    public void TriangleGraph_ReturnsAllPaths()
    {
        var builder = new RelaxedGraphBuilder();
        builder.TryAddEdge("A", "B");
        builder.TryAddEdge("B", "C");
        builder.TryAddEdge("C", "A");

        var result = ShortestPathsFinder.FindAllShortestPaths(builder.Graph);

        Assert.That(result.Get("A", "B").Single().Count, Is.EqualTo(1));
        Assert.That(result.Get("A", "C").Single().Count, Is.EqualTo(2));
        Assert.That(result.Get("B", "C").Single().Count, Is.EqualTo(1));
        Assert.That(result.Get("B", "A").Single().Count, Is.EqualTo(2));
        Assert.That(result.Get("C", "A").Single().Count, Is.EqualTo(1));
        Assert.That(result.Get("C", "B").Single().Count, Is.EqualTo(2));
    }

    [Test]
    public void DisconnectedGraph_ReturnsPartialPaths()
    {
        var builder = new RelaxedGraphBuilder();
        builder.TryAddEdge("A", "B");
        builder.TryAddEdge("C", "D");

        var result = ShortestPathsFinder.FindAllShortestPaths(builder.Graph);

        Assert.That(result.Get("A", "B").Single().Count, Is.EqualTo(1));
        Assert.That(result.Get("C", "D").Single().Count, Is.EqualTo(1));
        Assert.That(result.Get("A", "C"), Is.Empty);
    }

    [Test]
    public void WeightedEdges_IgnoresWeights()
    {
        var builder = new RelaxedGraphBuilder();
        builder.TryAddEdge("A", "B");
        builder.TryAddEdge("B", "C");
        builder.TryAddEdge("A", "C", 3); // Weight ignored, still 1 hop

        var result = ShortestPathsFinder.FindAllShortestPaths(builder.Graph);

        Assert.That(result.Get("A", "B").Single().Count, Is.EqualTo(1));
        Assert.That(result.Get("A", "C").Single().Count, Is.EqualTo(1));
    }

    [Test]
    public void DiamondGraph_FindsMultipleShortestPaths()
    {
        var builder = new RelaxedGraphBuilder();
        builder.TryAddEdge("A", "B");
        builder.TryAddEdge("A", "C");
        builder.TryAddEdge("B", "D");
        builder.TryAddEdge("C", "D");

        var result = ShortestPathsFinder.FindAllShortestPaths(builder.Graph);

        // Expected paths
        var pathsAD = result.Paths.Where(p => p[0].Source.Id == "A" && p.Last().Target.Id == "D").ToList();
        Assert.That(pathsAD.Count, Is.EqualTo(2), "Should find 2 shortest paths from A to D");

        // Check both paths exist
        var path1 = pathsAD.FirstOrDefault(p => p.Count == 2 && p[0].Target.Id == "B" && p[1].Target.Id == "D");
        var path2 = pathsAD.FirstOrDefault(p => p.Count == 2 && p[0].Target.Id == "C" && p[1].Target.Id == "D");

        Assert.That(path1, Is.Not.Null, "Path A -> B -> D should exist");
        Assert.That(path2, Is.Not.Null, "Path A -> C -> D should exist");

        // Verify other paths
        var pathsAB = result.Paths.Count(p => p[0].Source.Id == "A" && p.Last().Target.Id == "B");
        var pathsAC = result.Paths.Count(p => p[0].Source.Id == "A" && p.Last().Target.Id == "C");
        var pathsBD = result.Paths.Count(p => p[0].Source.Id == "B" && p.Last().Target.Id == "D");
        var pathsCD = result.Paths.Count(p => p[0].Source.Id == "C" && p.Last().Target.Id == "D");

        Assert.That(pathsAB, Is.EqualTo(1), "A -> B should have 1 path");
        Assert.That(pathsAC, Is.EqualTo(1), "A -> C should have 1 path");
        Assert.That(pathsBD, Is.EqualTo(1), "B -> D should have 1 path");
        Assert.That(pathsCD, Is.EqualTo(1), "C -> D should have 1 path");
    }
}