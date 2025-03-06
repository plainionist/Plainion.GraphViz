using System.Linq;
using NUnit.Framework;
using Plainion.Graphs.Undirected;
using Plainion.GraphViz.Modules.Metrics.Algorithms;

namespace Plainion.GraphViz.Modules.Metrics.Tests;

[TestFixture]
internal class UndirectedGraphTests
{
    [Test]
    public void EmptyGraph()
    {
        var builder = new RelaxedGraphBuilder();

        var edges = builder.Nodes.Edges().Select(x => (x.Item1.Id, x.Item2.Id)).ToList();

        Assert.That(edges, Is.Empty);
    }

    [Test]
    public void SingleEdge()
    {
        var builder = new RelaxedGraphBuilder();
        builder.TryAddEdge("A", "B");

        var edges = builder.Nodes.Edges().Select(x => (x.Item1.Id, x.Item2.Id)).ToList();

        Assert.That(edges, Is.EquivalentTo([("A", "B")]));
    }

    [Test]
    public void LineGraph()
    {
        var builder = new RelaxedGraphBuilder();
        builder.TryAddEdge("A", "B");
        builder.TryAddEdge("B", "C");
        builder.TryAddEdge("C", "D");

        var edges = builder.Nodes.Edges().Select(x => (x.Item1.Id, x.Item2.Id)).ToList();

        Assert.That(edges, Is.EquivalentTo([("A", "B"), ("B", "C"), ("C", "D")]));
    }

    [Test]
    public void TriangleGraph()
    {
        var builder = new RelaxedGraphBuilder();
        builder.TryAddEdge("A", "B");
        builder.TryAddEdge("B", "C");
        builder.TryAddEdge("C", "A");

        var edges = builder.Nodes.Edges().Select(x => (x.Item1.Id, x.Item2.Id)).ToList();

        Assert.That(edges, Is.EquivalentTo([("A", "B"), ("A", "C"), ("B", "C")]));
    }

    [Test]
    public void StarGraph()
    {
        var builder = new RelaxedGraphBuilder();
        builder.TryAddEdge("A", "B");
        builder.TryAddEdge("A", "C");
        builder.TryAddEdge("A", "D");
        builder.TryAddEdge("A", "E");

        var edges = builder.Nodes.Edges().Select(x => (x.Item1.Id, x.Item2.Id)).ToList();

        Assert.That(edges, Is.EquivalentTo([("A", "B"), ("A", "C"), ("A", "D"), ("A", "E")]));
    }

    [Test]
    public void ComplexGraph()
    {
        var builder = new RelaxedGraphBuilder();
        builder.TryAddEdge("A", "B");
        builder.TryAddEdge("A", "C");
        builder.TryAddEdge("B", "D");
        builder.TryAddEdge("C", "D");
        builder.TryAddEdge("C", "E");
        builder.TryAddEdge("D", "E");
        builder.TryAddEdge("E", "F");

        var edges = builder.Nodes.Edges().Select(x => (x.Item1.Id, x.Item2.Id)).ToList();

        Assert.That(edges, Is.EquivalentTo([("A", "B"), ("A", "C"), ("B", "D"), ("C", "D"), ("C", "E"), ("D", "E"), ("E", "F")]));
    }

    [Test]
    public void DisconnectedGraph()
    {
        var builder = new RelaxedGraphBuilder();
        builder.TryAddEdge("A", "B");
        builder.TryAddEdge("C", "D");

        var edges = builder.Nodes.Edges().Select(x => (x.Item1.Id, x.Item2.Id)).ToList();

        Assert.That(edges, Is.EquivalentTo([("A", "B"), ("C", "D")]));
    }

    [Test]
    public void DiamondGraph()
    {
        var builder = new RelaxedGraphBuilder();
        builder.TryAddEdge("A", "B");
        builder.TryAddEdge("A", "C");
        builder.TryAddEdge("B", "D");
        builder.TryAddEdge("C", "D");

        var edges = builder.Nodes.Edges().Select(x => (x.Item1.Id, x.Item2.Id)).ToList();

        Assert.That(edges, Is.EquivalentTo([("A", "B"), ("A", "C"), ("B", "D"), ("C", "D")]));
    }
}

