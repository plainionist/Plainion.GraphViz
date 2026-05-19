using NUnit.Framework;
using Plainion.Graphs.Projections;

namespace Plainion.Graphs.Algorithms.Tests;

[TestFixture]
public class RemoveNodesWithoutCrossClusterEdgesTests
{
    // Rule: A node in a cluster is kept only if it has at least one direct edge to a node in a different cluster

    [Test]
    public void NodesWithCrossClusterEdge_AreKept()
    {
        var builder = new RelaxedGraphBuilder();
        builder.TryAddEdge("a", "b");
        builder.TryAddCluster("c1", new[] { "a" });
        builder.TryAddCluster("c2", new[] { "b" });

        var projections = new NullGraphProjections(builder.Graph);
        var mask = new RemoveNodesWithoutCrossClusterEdges(projections).Compute();

        // "a" has outgoing cross-cluster edge, "b" has incoming cross-cluster edge
        Assert.That(mask.IsSet(projections.GetNode("a")), Is.Null);
        Assert.That(mask.IsSet(projections.GetNode("b")), Is.Null);
    }

    [Test]
    public void NodeWithOnlyIntraClusterEdges_IsRemoved()
    {
        // c1: a→b→c, c2: d; c→d is the only cross-cluster edge
        var builder = new RelaxedGraphBuilder();
        builder.TryAddEdge("a", "b");
        builder.TryAddEdge("b", "c");
        builder.TryAddEdge("c", "d");
        builder.TryAddCluster("c1", new[] { "a", "b", "c" });
        builder.TryAddCluster("c2", new[] { "d" });

        var projections = new NullGraphProjections(builder.Graph);
        var mask = new RemoveNodesWithoutCrossClusterEdges(projections).Compute();

        // a and b only have intra-cluster edges → hidden
        Assert.That(mask.IsSet(projections.GetNode("a")), Is.False);
        Assert.That(mask.IsSet(projections.GetNode("b")), Is.False);
        // c has out-edge to d (different cluster) → kept
        Assert.That(mask.IsSet(projections.GetNode("c")), Is.Null);
        // d has in-edge from c (different cluster) → kept
        Assert.That(mask.IsSet(projections.GetNode("d")), Is.Null);
    }

    [Test]
    public void NodeWithoutAnyEdges_IsRemoved()
    {
        var builder = new RelaxedGraphBuilder();
        builder.TryAddNode("a");
        builder.TryAddEdge("b", "c");
        builder.TryAddCluster("c1", new[] { "a", "b" });
        builder.TryAddCluster("c2", new[] { "c" });

        var projections = new NullGraphProjections(builder.Graph);
        var mask = new RemoveNodesWithoutCrossClusterEdges(projections).Compute();

        Assert.That(mask.IsSet(projections.GetNode("a")), Is.False);
        Assert.That(mask.IsSet(projections.GetNode("b")), Is.Null);
    }

    // Rule: Nodes not belonging to any cluster are always removed

    [Test]
    public void FreeNodeWithEdgesToClusterNodes_IsRemoved()
    {
        var builder = new RelaxedGraphBuilder();
        builder.TryAddEdge("orphan", "a");
        builder.TryAddCluster("c1", new[] { "a" });

        var projections = new NullGraphProjections(builder.Graph);
        var mask = new RemoveNodesWithoutCrossClusterEdges(projections).Compute();

        Assert.That(mask.IsSet(projections.GetNode("orphan")), Is.False);
    }

    [Test]
    public void FreeNodeWithoutEdges_IsRemoved()
    {
        var builder = new RelaxedGraphBuilder();
        builder.TryAddNode("z");

        var projections = new NullGraphProjections(builder.Graph);
        var mask = new RemoveNodesWithoutCrossClusterEdges(projections).Compute();

        Assert.That(mask.IsSet(projections.GetNode("z")), Is.False);
    }

    // Rule: Folded clusters are temporarily unfolded for analysis

    [Test]
    public void FoldedCluster_NodesAnalyzedIndividually()
    {
        // c1 (folded): a→c (cross-cluster), a→b (intra); c2: c
        var builder = new RelaxedGraphBuilder();
        builder.TryAddEdge("a", "c");
        builder.TryAddEdge("a", "b");
        builder.TryAddCluster("c1", new[] { "a", "b" });
        builder.TryAddCluster("c2", new[] { "c" });

        var projections = new NullGraphProjections(builder.Graph);
        projections.ClusterFolding.Toggle("c1");

        var mask = new RemoveNodesWithoutCrossClusterEdges(projections).Compute();

        Assert.That(mask.IsSet(projections.GetNode("a")), Is.Null);   // has cross-cluster edge
        Assert.That(mask.IsSet(projections.GetNode("b")), Is.False);  // only intra-cluster
        Assert.That(mask.IsSet(projections.GetNode("c")), Is.Null);   // has cross-cluster edge
    }

    [Test]
    public void AllFoldedClusters_Processed()
    {
        // c1 (folded): a, b; c2 (folded): c, d; a→c cross-cluster
        var builder = new RelaxedGraphBuilder();
        builder.TryAddEdge("a", "c");
        builder.TryAddCluster("c1", new[] { "a", "b" });
        builder.TryAddCluster("c2", new[] { "c", "d" });

        var projections = new NullGraphProjections(builder.Graph);
        projections.ClusterFolding.Toggle("c1");
        projections.ClusterFolding.Toggle("c2");

        var mask = new RemoveNodesWithoutCrossClusterEdges(projections).Compute();

        Assert.That(mask.IsSet(projections.GetNode("a")), Is.Null);   // has cross-cluster out-edge
        Assert.That(mask.IsSet(projections.GetNode("b")), Is.False);  // no cross-cluster edge
        Assert.That(mask.IsSet(projections.GetNode("c")), Is.Null);   // has cross-cluster in-edge
        Assert.That(mask.IsSet(projections.GetNode("d")), Is.False);  // no cross-cluster edge
    }

    [Test]
    public void FoldedClusters_RestoredAfterCompute()
    {
        var builder = new RelaxedGraphBuilder();
        builder.TryAddEdge("a", "b");
        builder.TryAddCluster("c1", new[] { "a" });
        builder.TryAddCluster("c2", new[] { "b" });

        var projections = new NullGraphProjections(builder.Graph);
        projections.ClusterFolding.Toggle("c1");
        projections.ClusterFolding.Toggle("c2");

        new RemoveNodesWithoutCrossClusterEdges(projections).Compute();

        Assert.That(projections.ClusterFolding.Clusters, Is.EquivalentTo(new[] { "c1", "c2" }));
    }

    // Rule: Already hidden nodes are not processed

    [Test]
    public void HiddenNode_NotAffectedByAlgorithm()
    {
        var builder = new RelaxedGraphBuilder();
        builder.TryAddEdge("a", "b");
        builder.TryAddCluster("c1", new[] { "a" });
        builder.TryAddCluster("c2", new[] { "b" });

        // Use a custom picking that returns false for "a" (simulates "a" already hidden)
        var customProjections = new CustomPickingProjections(builder.Graph, id => id != "a");
        var mask = new RemoveNodesWithoutCrossClusterEdges(customProjections).Compute();

        // "a" was already hidden → not touched by the mask (IsSet returns null = not in mask)
        Assert.That(mask.IsSet(customProjections.GetNode("a")), Is.Null);
    }

    // Rule: The result is a hide-mask

    [Test]
    public void MaskLabel_IsCorrect()
    {
        var builder = new RelaxedGraphBuilder();
        var projections = new NullGraphProjections(builder.Graph);

        var mask = new RemoveNodesWithoutCrossClusterEdges(projections).Compute();

        Assert.That(mask.Label, Is.EqualTo("Nodes without cross-cluster edges"));
    }

    [Test]
    public void MaskIsHideMask()
    {
        var builder = new RelaxedGraphBuilder();
        var projections = new NullGraphProjections(builder.Graph);

        var mask = new RemoveNodesWithoutCrossClusterEdges(projections).Compute();

        Assert.That(mask.IsShowMask, Is.False);
    }
}
