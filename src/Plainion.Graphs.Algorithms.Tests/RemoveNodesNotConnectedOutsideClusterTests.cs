using NUnit.Framework;

namespace Plainion.Graphs.Algorithms.Tests;

[TestFixture]
public class RemoveNodesNotConnectedOutsideClusterTests
{
    [Test]
    public void DirectCrossClusterEdge()
    {
        var builder = new RelaxedGraphBuilder();
        builder.TryAddEdge("a", "b");
        builder.TryAddEdge("x", "y");
        builder.TryAddCluster("c1", new[] { "a", "x" });
        builder.TryAddCluster("c2", new[] { "b", "y" });

        var projections = new GraphProjections(builder.Graph);

        var algo = new RemoveNodesNotConnectedOutsideCluster(projections, SiblingsType.Any);

        var mask = algo.Compute(projections.GetCluster("c1"));

        Assert.That(mask.IsSet(projections.GetNode("a")), Is.Null);
        Assert.That(mask.IsSet(projections.GetNode("b")), Is.Null);
        Assert.That(mask.IsSet(projections.GetNode("x")), Is.Null);
        Assert.That(mask.IsSet(projections.GetNode("y")), Is.Null);
    }

    [Test]
    public void DirectCrossClusterEdge_NotReachableFromOutside()
    {
        var builder = new RelaxedGraphBuilder();
        builder.TryAddEdge("a", "b");
        builder.TryAddEdge("x", "y");
        builder.TryAddCluster("c1", new[] { "a" });
        builder.TryAddCluster("c2", new[] { "b", "x", "y" });

        var projections = new GraphProjections(builder.Graph);

        var algo = new RemoveNodesNotConnectedOutsideCluster(projections, SiblingsType.Sources);

        var mask = algo.Compute(projections.GetCluster("c2"));

        Assert.That(mask.IsSet(projections.GetNode("a")), Is.Null);
        Assert.That(mask.IsSet(projections.GetNode("b")), Is.Null);
        Assert.That(mask.IsSet(projections.GetNode("x")), Is.EqualTo(false));
        Assert.That(mask.IsSet(projections.GetNode("y")), Is.EqualTo(false));
    }

    [Test]
    public void DirectCrossClusterEdge_ClustersFolded_NotReachableFromOutside()
    {
        var builder = new RelaxedGraphBuilder();
        builder.TryAddEdge("a", "b");
        builder.TryAddEdge("x", "y");
        builder.TryAddCluster("c1", new[] { "a" });
        builder.TryAddCluster("c2", new[] { "b", "x", "y" });

        var projections = new GraphProjections(builder.Graph);
        projections.ClusterFolding.Toggle("c1");
        projections.ClusterFolding.Toggle("c2");

        var algo = new RemoveNodesNotConnectedOutsideCluster(projections, SiblingsType.Sources);

        var mask = algo.Compute(projections.GetCluster("c2"));

        Assert.That(mask.IsSet(projections.GetNode("a")), Is.Null);
        Assert.That(mask.IsSet(projections.GetNode("b")), Is.Null);
        Assert.That(mask.IsSet(projections.GetNode("x")), Is.EqualTo(false));
        Assert.That(mask.IsSet(projections.GetNode("y")), Is.EqualTo(false));
    }

    [Test]
    public void IndirectCrossClusterEdge()
    {
        var builder = new RelaxedGraphBuilder();
        builder.TryAddEdge("a", "b");
        builder.TryAddEdge("b", "c");
        builder.TryAddCluster("c1", new[] { "a", "b" });
        builder.TryAddCluster("c2", new[] { "c" });

        var projections = new GraphProjections(builder.Graph);

        var algo = new RemoveNodesNotConnectedOutsideCluster(projections, SiblingsType.Any);

        var mask = algo.Compute(projections.GetCluster("c1"));

        Assert.That(mask.IsSet(projections.GetNode("a")), Is.Null);
        Assert.That(mask.IsSet(projections.GetNode("b")), Is.Null);
        Assert.That(mask.IsSet(projections.GetNode("c")), Is.Null);
    }

    [Test]
    public void OnlyInnerClusterEdge()
    {
        var builder = new RelaxedGraphBuilder();
        builder.TryAddEdge("a", "b");
        builder.TryAddEdge("b", "c");
        builder.TryAddCluster("c1", new[] { "a", "b", "c" });

        var projections = new GraphProjections(builder.Graph);

        var algo = new RemoveNodesNotConnectedOutsideCluster(projections, SiblingsType.Any);

        var mask = algo.Compute(projections.GetCluster("c1"));

        Assert.That(mask.IsSet(projections.GetNode("a")), Is.False);
        Assert.That(mask.IsSet(projections.GetNode("b")), Is.False);
        Assert.That(mask.IsSet(projections.GetNode("c")), Is.False);
    }

    [Test]
    public void InnerClusterCycle()
    {
        var builder = new RelaxedGraphBuilder();
        builder.TryAddEdge("a", "b");
        builder.TryAddEdge("b", "a");
        builder.TryAddEdge("x", "y");
        builder.TryAddEdge("y", "z");
        builder.TryAddEdge("z", "x");
        builder.TryAddCluster("c1", new[] { "a", "b", "x", "y", "z" });

        var projections = new GraphProjections(builder.Graph);

        var algo = new RemoveNodesNotConnectedOutsideCluster(projections, SiblingsType.Any);

        var mask = algo.Compute(projections.GetCluster("c1"));

        Assert.That(mask.IsSet(projections.GetNode("a")), Is.False);
        Assert.That(mask.IsSet(projections.GetNode("b")), Is.False);
        Assert.That(mask.IsSet(projections.GetNode("x")), Is.False);
        Assert.That(mask.IsSet(projections.GetNode("y")), Is.False);
        Assert.That(mask.IsSet(projections.GetNode("z")), Is.False);
    }

    [Test]
    public void NodeWithoutEdges()
    {
        var builder = new RelaxedGraphBuilder();
        builder.TryAddNode("a");
        builder.TryAddNode("b");
        builder.TryAddCluster("c1", new[] { "a", "b" });

        var projections = new GraphProjections(builder.Graph);

        var algo = new RemoveNodesNotConnectedOutsideCluster(projections, SiblingsType.Any);

        var mask = algo.Compute(projections.GetCluster("c1"));

        Assert.That(mask.IsSet(projections.GetNode("a")), Is.False);
        Assert.That(mask.IsSet(projections.GetNode("b")), Is.False);
    }
}
