using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using Plainion.Graphs;


namespace Plainion.GraphViz.Modules.Metrics.Tests;

[TestFixture]
public class ShortestPathsFinderTests
{
    [Test]
    public void NoEdges_ReturnsNoPaths()
    {
        var builder = new RelaxedGraphBuilder();
        builder.TryAddEdge("A", "B");
        var graph = builder.Graph;

        var result = ShortestPathsFinder.FindAllShortestPaths(graph);

        var abPath = result.Paths.FirstOrDefault(p => p[0].Source.Id == "A" && p.Last().Target.Id == "B");
        Assert.That(abPath, Is.Not.Null);
        Assert.That(abPath.Count, Is.EqualTo(1));
        Assert.That(abPath[0].Source.Id, Is.EqualTo("A"));
        Assert.That(abPath[0].Target.Id, Is.EqualTo("B"));
        Assert.That(ShortestPathsResult.GetDistance(abPath), Is.EqualTo(1));

        var bPaths = result.Paths.Any(p => p[0].Source.Id == "B");
        Assert.That(bPaths, Is.False); // B has no outgoing paths
    }

    [Test]
    public void TriangleGraph_ReturnsAllPaths()
    {
        var builder = new RelaxedGraphBuilder();
        builder.TryAddEdge("A", "B");
        builder.TryAddEdge("B", "C");
        builder.TryAddEdge("C", "A");

        var result = ShortestPathsFinder.FindAllShortestPaths(builder.Graph);

        // A -> B -> C
        var ab = result.Paths.First(p => p[0].Source.Id == "A" && p.Last().Target.Id == "B");
        Assert.That(ab.Count, Is.EqualTo(1));
        Assert.That(ShortestPathsResult.GetDistance(ab), Is.EqualTo(1));
        var ac = result.Paths.First(p => p[0].Source.Id == "A" && p.Last().Target.Id == "C");
        Assert.That(ac.Count, Is.EqualTo(2));
        Assert.That(ShortestPathsResult.GetDistance(ac), Is.EqualTo(2));

        // B -> C -> A
        var bc = result.Paths.First(p => p[0].Source.Id == "B" && p.Last().Target.Id == "C");
        Assert.That(bc.Count, Is.EqualTo(1));
        Assert.That(ShortestPathsResult.GetDistance(bc), Is.EqualTo(1));
        var ba = result.Paths.First(p => p[0].Source.Id == "B" && p.Last().Target.Id == "A");
        Assert.That(ba.Count, Is.EqualTo(2));
        Assert.That(ShortestPathsResult.GetDistance(ba), Is.EqualTo(2));

        // C -> A -> B
        var ca = result.Paths.First(p => p[0].Source.Id == "C" && p.Last().Target.Id == "A");
        Assert.That(ca.Count, Is.EqualTo(1));
        Assert.That(ShortestPathsResult.GetDistance(ca), Is.EqualTo(1));
        var cb = result.Paths.First(p => p[0].Source.Id == "C" && p.Last().Target.Id == "B");
        Assert.That(cb.Count, Is.EqualTo(2));
        Assert.That(ShortestPathsResult.GetDistance(cb), Is.EqualTo(2));
    }

    [Test]
    public void DisconnectedGraph_ReturnsPartialPaths()
    {
        var builder = new RelaxedGraphBuilder();
        builder.TryAddEdge("A", "B");
        builder.TryAddEdge("C", "D");

        var result = ShortestPathsFinder.FindAllShortestPaths(builder.Graph);

        var ab = result.Paths.FirstOrDefault(p => p[0].Source.Id == "A" && p.Last().Target.Id == "B");
        Assert.That(ab, Is.Not.Null);
        Assert.That(ShortestPathsResult.GetDistance(ab), Is.EqualTo(1));

        var cd = result.Paths.FirstOrDefault(p => p[0].Source.Id == "C" && p.Last().Target.Id == "D");
        Assert.That(cd, Is.Not.Null);
        Assert.That(ShortestPathsResult.GetDistance(cd), Is.EqualTo(1));

        var ac = result.Paths.Any(p => p[0].Source.Id == "A" && p.Last().Target.Id == "C");
        Assert.That(ac, Is.False);
    }

    [Test]
    public void WeightedEdges_ReturnsCorrectDistances()
    {
        var builder = new RelaxedGraphBuilder();
        builder.TryAddEdge("A", "B");
        builder.TryAddEdge("B", "C");
        builder.TryAddEdge("A", "C", 3);

        var result = ShortestPathsFinder.FindAllShortestPaths(builder.Graph);

        var ab = result.Paths.First(p => p[0].Source.Id == "A" && p.Last().Target.Id == "B");
        Assert.That(ShortestPathsResult.GetDistance(ab), Is.EqualTo(1));
        var ac = result.Paths.First(p => p[0].Source.Id == "A" && p.Last().Target.Id == "C");
        Assert.That(ac.Count, Is.EqualTo(2)); // A -> B -> C
        Assert.That(ShortestPathsResult.GetDistance(ac), Is.EqualTo(2)); // Shorter than direct A -> C (3)
    }
}