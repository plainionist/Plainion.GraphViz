using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using Plainion.Graphs;
using Plainion.GraphViz.Modules.Metrics.Algorithms;

namespace Plainion.GraphViz.Modules.Metrics.Tests;

[TestFixture]
public class GraphCycleDetectorTests
{
    [Test]
    public void NoCycles_ReturnsEmptyList()
    {
        // A -> B -> C (no cycles)
        var builder = new RelaxedGraphBuilder();
        builder.TryAddEdge("A", "B");
        builder.TryAddEdge("B", "C");

        var cycles = DetectCycles(builder);

        Assert.That(cycles, Is.Empty);
    }

    [Test]
    public void SingleCycle_ReturnsOneCycle()
    {
        // A -> B -> C -> A
        var builder = new RelaxedGraphBuilder();
        builder.TryAddEdge("A", "B");
        builder.TryAddEdge("B", "C");
        builder.TryAddEdge("C", "A");

        var cycles = DetectCycles(builder);

        Assert.That(cycles.Count, Is.EqualTo(1));
        AssertThatCyclesAreEqual(cycles[0], new List<string> { "A", "B", "C", "A" });
    }

    [Test]
    public void MultipleCycles_ReturnsAllCycles()
    {
        // A -> B -> C -> B -> D -> A
        // cycle: B -> C -> B, cycle A -> B -> D -> A
        var builder = new RelaxedGraphBuilder();
        builder.TryAddEdge("A", "B");
        builder.TryAddEdge("B", "C");
        builder.TryAddEdge("C", "B");
        builder.TryAddEdge("B", "D");
        builder.TryAddEdge("D", "A");

        var cycles = DetectCycles(builder);

        Assert.That(cycles.Count, Is.EqualTo(2));
        AssertThatCyclesAreEqual(cycles[0], new List<string> { "B", "C", "B" });
        AssertThatCyclesAreEqual(cycles[1], new List<string> { "A", "B", "D", "A" });
    }

    [Test]
    public void SelfLoop_ReturnsSelfCycle()
    {
        // A -> A
        var builder = new RelaxedGraphBuilder();
        builder.TryAddEdge("A", "A");

        var cycles = DetectCycles(builder);

        Assert.That(cycles.Count, Is.EqualTo(1));
        Assert.That(cycles[0], Is.EqualTo(new List<string> { "A", "A" }));
    }

    private static List<List<string>> DetectCycles(RelaxedGraphBuilder builder) =>
        CycleFinder.FindAllCycles(builder.Graph)
            .Select(x => new List<string> { x.Start.Id }.Concat(x.Path.Select(x => x.Id)).ToList())
            .OrderBy(x => x.Count)
            .ToList();

    private static void AssertThatCyclesAreEqual(List<string> actual, List<string> expected)
    {
        Assert.That(actual.Count, Is.EqualTo(expected.Count), "Cycles have different length");

        // Double the expected cycle (skip beginning - duplicated - node) to handle rotations
        var doubledExpected = expected.Concat(expected.Skip(1)).ToList();

        var actualStr = string.Join(",", actual);
        var expectedStr = string.Join(",", doubledExpected);
        Assert.That(expectedStr, Contains.Substring(actualStr));
    }
}