using NUnit.Framework;
using Plainion.GraphViz.Algorithms;
using Plainion.GraphViz.Model;
using Plainion.GraphViz.Presentation;

namespace Plainion.GraphViz.Tests.Algorithms
{
    [TestFixture]
    public class RemoveNodesNotConnectedOutsideClusterTests
    {
        [Test]
        public void SimpleCrossClusterEdge()
        {
            var builder = new RelaxedGraphBuilder();
            builder.TryAddEdge("a", "b");
            builder.TryAddCluster("c1", new[] { "a" });
            builder.TryAddCluster("c2", new[] { "b" });

            var presentation = new GraphPresentation(builder.Graph);

            var algo = new RemoveNodesNotConnectedOutsideCluster(presentation, SiblingsType.Any);

            var mask = algo.Compute(presentation.GetCluster("c1"));

            Assert.That(mask.IsSet(presentation.GetNode("a")), Is.True);
        }

        [Test]
        public void OnlyInnerClusterEdge()
        {
            var builder = new RelaxedGraphBuilder();
            builder.TryAddEdge("a", "b");
            builder.TryAddCluster("c1", new[] { "a", "b" });

            var presentation = new GraphPresentation(builder.Graph);

            var algo = new RemoveNodesNotConnectedOutsideCluster(presentation, SiblingsType.Any);

            var mask = algo.Compute(presentation.GetCluster("c1"));

            Assert.That(mask.IsSet(presentation.GetNode("a")), Is.True);
            Assert.That(mask.IsSet(presentation.GetNode("b")), Is.True);
        }

        [Test]
        public void NodeWithoutEdges()
        {
            var builder = new RelaxedGraphBuilder();
            builder.TryAddNode("a");
            builder.TryAddCluster("c1", new[] { "a" });

            var presentation = new GraphPresentation(builder.Graph);

            var algo = new RemoveNodesNotConnectedOutsideCluster(presentation, SiblingsType.Any);

            var mask = algo.Compute(presentation.GetCluster("c1"));

            Assert.That(mask.IsSet(presentation.GetNode("a")), Is.True);
        }
    }
}
