using System.Linq;
using NUnit.Framework;
using Plainion.Graphs.Undirected;
using Plainion.GraphViz.Modules.Metrics.Algorithms;

namespace Plainion.GraphViz.Modules.Metrics.Tests;

[TestFixture]
public class UndirectedShortestPathFinderTests
{
    [Test]
    public void SingleEdge()
    {
        var builder = new RelaxedGraphBuilder();
        builder.TryAddEdge("A", "B");

        var result = UndirectedShortestPathsFinder.FindAllShortestPaths(builder.Graph);

        var paths = result.Get("A", "B");
        Assert.That(paths.Count, Is.EqualTo(1));
        Assert.That(paths.Single().Distance, Is.EqualTo(1));
    }

    [Test]
    public void TriangleGraph()
    {
        var builder = new RelaxedGraphBuilder();
        builder.TryAddEdge("A", "B");
        builder.TryAddEdge("B", "C");
        builder.TryAddEdge("C", "A");

        var result = UndirectedShortestPathsFinder.FindAllShortestPaths(builder.Graph);

        Assert.That(result.Get("A", "B").Single().Distance, Is.EqualTo(1));
        Assert.That(result.Get("A", "C").Single().Distance, Is.EqualTo(1));
        Assert.That(result.Get("B", "C").Single().Distance, Is.EqualTo(1));
    }

    [Test]
    public void DisconnectedGraph()
    {
        var builder = new RelaxedGraphBuilder();
        builder.TryAddEdge("A", "B");
        builder.TryAddEdge("C", "D");

        var result = UndirectedShortestPathsFinder.FindAllShortestPaths(builder.Graph);

        Assert.That(result.Get("A", "B").Single().Distance, Is.EqualTo(1));
        Assert.That(result.Get("C", "D").Single().Distance, Is.EqualTo(1));
        Assert.That(result.Get("A", "C"), Is.Empty);
    }

    [Test]
    public void DiamondGraph()
    {
        var builder = new RelaxedGraphBuilder();
        builder.TryAddEdge("A", "B");
        builder.TryAddEdge("A", "C");
        builder.TryAddEdge("B", "D");
        builder.TryAddEdge("C", "D");

        var result = UndirectedShortestPathsFinder.FindAllShortestPaths(builder.Graph);

        var pathsAD = result.Get("A", "D");
        Assert.That(pathsAD.Count, Is.EqualTo(1), "Should find 1 shortest path from A to D"); // Single path per pair
        Assert.That(pathsAD.Single().Distance, Is.EqualTo(2));

        Assert.That(result.Get("A", "B").Single().Distance, Is.EqualTo(1));
        Assert.That(result.Get("A", "C").Single().Distance, Is.EqualTo(1));
        Assert.That(result.Get("B", "D").Single().Distance, Is.EqualTo(1));
        Assert.That(result.Get("C", "D").Single().Distance, Is.EqualTo(1));
    }
}


