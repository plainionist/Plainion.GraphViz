using System.Linq;
using NUnit.Framework;
using Plainion.Graphs;

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
}