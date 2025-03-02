using NUnit.Framework;
using Plainion.GraphViz.Modules.CodeInspection.Packaging.Spec;

namespace Plainion.GraphViz.Modules.CodeInspection.Packaging.Tests.Spec;

[TestFixture]
class ClusterMatchingTests
{
    [Test]
    public void NoWildcard_NoExclusion()
    {
        var cluster = new Cluster { Name = "C1", Id = "42" };
        cluster.Patterns.Add(new Include { Pattern = "App.Feature.A" });

        Assert.That(cluster.Matches("App.Feature.A"), Is.True);
        Assert.That(cluster.Matches("App.Feature.A.Contracts"), Is.False);
        Assert.That(cluster.Matches("Feature.A"), Is.False);
    }

    [Test]
    public void NoWildcard_Exclusion()
    {
        var cluster = new Cluster { Name = "C1", Id = "42" };
        cluster.Patterns.Add(new Include { Pattern = "App.Feature.A" });
        cluster.Patterns.Add(new Exclude { Pattern = "App.Feature.A.Contracts" });

        Assert.That(cluster.Matches("App.Feature.A"), Is.True);
        Assert.That(cluster.Matches("App.Feature.A.Core"), Is.False);
        Assert.That(cluster.Matches("App.Feature.A.Contracts"), Is.False);
        Assert.That(cluster.Matches("Feature.A"), Is.False);
    }

    [Test]
    public void Wildcard_NoExclusion()
    {
        var cluster = new Cluster { Name = "C1", Id = "42" };
        cluster.Patterns.Add(new Include { Pattern = "App.Feature.A*" });

        Assert.That(cluster.Matches("App.Feature.A"), Is.True);
        Assert.That(cluster.Matches("App.Feature.A.Contracts"), Is.True);
        Assert.That(cluster.Matches("Feature.A"), Is.False);
    }

    [Test]
    public void Wildcard_Exclusion()
    {
        var cluster = new Cluster { Name = "C1", Id = "42" };
        cluster.Patterns.Add(new Include { Pattern = "App.Feature.A*" });
        cluster.Patterns.Add(new Exclude { Pattern = "App.Feature.A.Contracts*" });

        Assert.That(cluster.Matches("App.Feature.A"), Is.True);
        Assert.That(cluster.Matches("App.Feature.A.Core"), Is.True);
        Assert.That(cluster.Matches("App.Feature.A.Contracts"), Is.False);
        Assert.That(cluster.Matches("Feature.A"), Is.False);
    }
}
