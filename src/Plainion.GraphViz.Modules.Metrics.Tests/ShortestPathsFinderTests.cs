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

        var result = ShortestPathsFinder.FindAllShortestPaths(builder.Graph);

        var ab = result.Paths.FirstOrDefault(p => p[0].Source.Id == "A" && p.Last().Target.Id == "B");
        Assert.That(ab, Is.Not.Null);
        Assert.That(ab.Count, Is.EqualTo(1));
        var bPaths = result.Paths.Any(p => p[0].Source.Id == "B");
        Assert.That(bPaths, Is.False);
    }

    [Test]
    public void TriangleGraph_ReturnsAllPaths()
    {
        var builder = new RelaxedGraphBuilder();
        builder.TryAddEdge("A", "B");
        builder.TryAddEdge("B", "C");
        builder.TryAddEdge("C", "A");

        var result = ShortestPathsFinder.FindAllShortestPaths(builder.Graph);

        var ab = result.Paths.First(p => p[0].Source.Id == "A" && p.Last().Target.Id == "B");
        Assert.That(ab.Count, Is.EqualTo(1));
        var ac = result.Paths.First(p => p[0].Source.Id == "A" && p.Last().Target.Id == "C");
        Assert.That(ac.Count, Is.EqualTo(2));
        var bc = result.Paths.First(p => p[0].Source.Id == "B" && p.Last().Target.Id == "C");
        Assert.That(bc.Count, Is.EqualTo(1));
        var ba = result.Paths.First(p => p[0].Source.Id == "B" && p.Last().Target.Id == "A");
        Assert.That(ba.Count, Is.EqualTo(2));
        var ca = result.Paths.First(p => p[0].Source.Id == "C" && p.Last().Target.Id == "A");
        Assert.That(ca.Count, Is.EqualTo(1));
        var cb = result.Paths.First(p => p[0].Source.Id == "C" && p.Last().Target.Id == "B");
        Assert.That(cb.Count, Is.EqualTo(2));
    }

    [Test]
    public void DisconnectedGraph_ReturnsPartialPaths()
    {
        var builder = new RelaxedGraphBuilder();
        builder.TryAddEdge("A", "B");
        builder.TryAddEdge("C", "D");

        var result = ShortestPathsFinder.FindAllShortestPaths(builder.Graph);

        var ab = result.Paths.FirstOrDefault(p => p[0].Source.Id == "A" && p.Last().Target.Id == "B");
        Assert.That(ab.Count, Is.EqualTo(1));
        var cd = result.Paths.FirstOrDefault(p => p[0].Source.Id == "C" && p.Last().Target.Id == "D");
        Assert.That(cd.Count, Is.EqualTo(1));
        var ac = result.Paths.Any(p => p[0].Source.Id == "A" && p.Last().Target.Id == "C");
        Assert.That(ac, Is.False);
    }

    [Test]
    public void WeightedEdges_IgnoresWeights()
    {
        var builder = new RelaxedGraphBuilder();
        builder.TryAddEdge("A", "B");
        builder.TryAddEdge("B", "C");
        builder.TryAddEdge("A", "C", 3); // Weight ignored, still 1 hop

        var result = ShortestPathsFinder.FindAllShortestPaths(builder.Graph);

        var ab = result.Paths.First(p => p[0].Source.Id == "A" && p.Last().Target.Id == "B");
        Assert.That(ab.Count, Is.EqualTo(1));
        var ac = result.Paths.First(p => p[0].Source.Id == "A" && p.Last().Target.Id == "C");
        Assert.That(ac.Count, Is.EqualTo(1)); // A -> C direct (1 hop), not A -> B -> C
        Assert.That(ac.Count, Is.EqualTo(1));
    }
}