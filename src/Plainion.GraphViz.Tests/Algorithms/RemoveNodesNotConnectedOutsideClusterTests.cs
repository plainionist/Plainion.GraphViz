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
        public void DirectCrossClusterEdge()
        {
            var builder = new RelaxedGraphBuilder();
            builder.TryAddEdge("a", "b");
            builder.TryAddEdge("x", "y");
            builder.TryAddCluster("c1", new[] { "a", "x" });
            builder.TryAddCluster("c2", new[] { "b", "y" });

            var presentation = new GraphPresentation(builder.Graph);

            var algo = new RemoveNodesNotConnectedOutsideCluster(presentation, SiblingsType.Any);

            var mask = algo.Compute(presentation.GetCluster("c1"));

            Assert.That(mask.IsSet(presentation.GetNode("a")), Is.Null);
            Assert.That(mask.IsSet(presentation.GetNode("b")), Is.Null);
            Assert.That(mask.IsSet(presentation.GetNode("x")), Is.Null);
            Assert.That(mask.IsSet(presentation.GetNode("y")), Is.Null);
        }

        [Test]
        public void IndirectCrossClusterEdge()
        {
            var builder = new RelaxedGraphBuilder();
            builder.TryAddEdge("a", "b");
            builder.TryAddEdge("b", "c");
            builder.TryAddCluster("c1", new[] { "a", "b" });
            builder.TryAddCluster("c2", new[] { "c" });

            var presentation = new GraphPresentation(builder.Graph);

            var algo = new RemoveNodesNotConnectedOutsideCluster(presentation, SiblingsType.Any);

            var mask = algo.Compute(presentation.GetCluster("c1"));

            Assert.That(mask.IsSet(presentation.GetNode("a")), Is.Null);
            Assert.That(mask.IsSet(presentation.GetNode("b")), Is.Null);
            Assert.That(mask.IsSet(presentation.GetNode("c")), Is.Null);
        }

        [Test]
        public void OnlyInnerClusterEdge()
        {
            var builder = new RelaxedGraphBuilder();
            builder.TryAddEdge("a", "b");
            builder.TryAddEdge("b", "c");
            builder.TryAddCluster("c1", new[] { "a", "b", "c" });

            var presentation = new GraphPresentation(builder.Graph);

            var algo = new RemoveNodesNotConnectedOutsideCluster(presentation, SiblingsType.Any);

            var mask = algo.Compute(presentation.GetCluster("c1"));

            Assert.That(mask.IsSet(presentation.GetNode("a")), Is.False);
            Assert.That(mask.IsSet(presentation.GetNode("b")), Is.False);
            Assert.That(mask.IsSet(presentation.GetNode("c")), Is.False);
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

            var presentation = new GraphPresentation(builder.Graph);

            var algo = new RemoveNodesNotConnectedOutsideCluster(presentation, SiblingsType.Any);

            var mask = algo.Compute(presentation.GetCluster("c1"));

            Assert.That(mask.IsSet(presentation.GetNode("a")), Is.False);
            Assert.That(mask.IsSet(presentation.GetNode("b")), Is.False);
            Assert.That(mask.IsSet(presentation.GetNode("x")), Is.False);
            Assert.That(mask.IsSet(presentation.GetNode("y")), Is.False);
            Assert.That(mask.IsSet(presentation.GetNode("z")), Is.False);
        }

        [Test]
        public void NodeWithoutEdges()
        {
            var builder = new RelaxedGraphBuilder();
            builder.TryAddNode("a");
            builder.TryAddNode("b");
            builder.TryAddCluster("c1", new[] { "a", "b" });

            var presentation = new GraphPresentation(builder.Graph);

            var algo = new RemoveNodesNotConnectedOutsideCluster(presentation, SiblingsType.Any);

            var mask = algo.Compute(presentation.GetCluster("c1"));

            Assert.That(mask.IsSet(presentation.GetNode("a")), Is.False);
            Assert.That(mask.IsSet(presentation.GetNode("b")), Is.False);
        }
    }
}
