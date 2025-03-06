using System.Linq;
using NUnit.Framework;
using Plainion.Graphs;
using Plainion.GraphViz.Modules.Metrics.Algorithms;

namespace Plainion.GraphViz.Modules.Metrics.Tests;

[TestFixture]
public class ShortestPathsFinderTests
{
    [Test]
    public void SingleEdge()
    {
        var builder = new RelaxedGraphBuilder();
        builder.TryAddEdge("A", "B");

        var result = ShortestPathsFinder.FindAllShortestPaths(builder.Graph);

        var paths = result.Get("A", "B");
        Assert.That(paths.Count, Is.EqualTo(1));
        Assert.That(paths.Single().Count, Is.EqualTo(1));
    }

    [Test]
    public void TriangleGraph()
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
    public void DisconnectedGraph()
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
    public void WeightedEdges()
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
    public void DiamondGraph()
    {
        var builder = new RelaxedGraphBuilder();
        builder.TryAddEdge("A", "B");
        builder.TryAddEdge("A", "C");
        builder.TryAddEdge("B", "D");
        builder.TryAddEdge("C", "D");

        var result = ShortestPathsFinder.FindAllShortestPaths(builder.Graph);

        // Expected paths
        var pathsAD = result.Get("A", "D");
        Assert.That(pathsAD.Count, Is.EqualTo(2), "Should find 2 shortest paths from A to D");

        // Check both paths exist
        var path1 = pathsAD.FirstOrDefault(p => p.Distance == 2 && p[0].Target.Id == "B" && p[1].Target.Id == "D");
        var path2 = pathsAD.FirstOrDefault(p => p.Distance == 2 && p[0].Target.Id == "C" && p[1].Target.Id == "D");
        Assert.That(path1, Is.Not.Null, "Path A -> B -> D should exist");
        Assert.That(path2, Is.Not.Null, "Path A -> C -> D should exist");

        Assert.That(result.Get("A", "B").Count, Is.EqualTo(1), "A -> B should have 1 path");
        Assert.That(result.Get("A", "C").Count, Is.EqualTo(1), "A -> C should have 1 path");
        Assert.That(result.Get("B", "D").Count, Is.EqualTo(1), "B -> D should have 1 path");
        Assert.That(result.Get("C", "D").Count, Is.EqualTo(1), "C -> D should have 1 path");
    }
}