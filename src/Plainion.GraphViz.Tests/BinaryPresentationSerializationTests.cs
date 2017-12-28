using System.Collections.Generic;
using System.IO;
using System.Linq;
using NUnit.Framework;
using Plainion.GraphViz.Model;
using Plainion.GraphViz.Presentation;

namespace Plainion.GraphViz.Tests
{
    [TestFixture]
    class GIVEN_APresentationToSerialize
    {
        private IGraphPresentation myPresentation;

        [SetUp]
        public void SetUp()
        {
            var builder = new RelaxedGraphBuilder();
            builder.TryAddEdge("a", "b");
            builder.TryAddEdge("a", "c");
            builder.TryAddNode("d");
            builder.TryAddCluster("X17", new[] {"b","c" });

            myPresentation = new GraphPresentation();
            myPresentation.Graph = builder.Graph;
        }

        [Test]
        public void WHEN_GraphHasEdges_THEN_EdgesAreSerialized()
        {
            var presentation = SerializeDeserialize(myPresentation);

            Assert.That(presentation, Is.Not.SameAs(myPresentation));
            Assert.That(presentation.Graph.Edges.Ids(), Is.EquivalentTo(myPresentation.Graph.Edges.Ids()));
        }

        [Test]
        public void WHEN_GraphHasNodesWithoutEdges_THEN_TheseNodesAreSerialized()
        {
            var presentation = SerializeDeserialize(myPresentation);

            Assert.That(presentation, Is.Not.SameAs(myPresentation));
            Assert.That(presentation.Graph.Nodes.Ids(), Contains.Item("d"));
        }

        [Test]
        public void WHEN_GraphHasClusters_THEN_ClustersAreSerialized()
        {
            var presentation = SerializeDeserialize(myPresentation);

            Assert.That(presentation, Is.Not.SameAs(myPresentation));
            Assert.That(presentation.Graph.Clusters.Ids(), Is.EquivalentTo(new string[] { "X17" }));
            Assert.That(presentation.Graph.Clusters.Single().Nodes.Ids(), Is.EquivalentTo(new string[] { "b","c" }));
        }

        private static IGraphPresentation SerializeDeserialize(IGraphPresentation presentation)
        {
            using (var stream = new MemoryStream())
            {
                using (var writer = new BinaryPresentationWriter(stream))
                {
                    writer.Write(presentation);
                }

                stream.Seek(0, SeekOrigin.Begin);

                using (var reader = new BinaryPresentationReader(stream))
                {
                    return reader.Read();
                }
            }
        }
    }

    static class GraphExtensions
    {
        public static IEnumerable<string> Ids(this IEnumerable<IGraphItem> self)
        {
            return self.Select(x => x.Id);
        }
    }
}
