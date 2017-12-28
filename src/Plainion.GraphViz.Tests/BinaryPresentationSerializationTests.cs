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
            builder.TryAddCluster("X17", new[] { "b", "c" });

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
            Assert.That(presentation.Graph.Clusters.Single().Nodes.Ids(), Is.EquivalentTo(new string[] { "b", "c" }));
        }

        [Test]
        public void WHEN_NodeMasksExist_THEN_MasksGetSerialized()
        {
            var module = myPresentation.GetModule<NodeMaskModule>();
            module.Push(new NodeMask(new[] { "b", "c" }));
            module.Push(new NodeMask(new[] { "a" }) { IsShowMask = false });

            var presentation = SerializeDeserialize(myPresentation);

            module = presentation.GetModule<NodeMaskModule>();

            Assert.That(module, Is.Not.SameAs(myPresentation.GetModule<NodeMaskModule>()));

            Assert.That(module.Items.ElementAt(0).IsShowMask, Is.False);
            Assert.That(((NodeMask)module.Items.ElementAt(0)).Values, Is.EqualTo(new[] { "a" }));

            Assert.That(module.Items.ElementAt(1).IsShowMask, Is.True);
            Assert.That(((NodeMask)module.Items.ElementAt(1)).Values, Is.EqualTo(new[] { "b", "c" }));
        }

        [Test]
        public void WHEN_AllNodesMasksExist_THEN_MasksGetSerialized()
        {
            var module = myPresentation.GetModule<NodeMaskModule>();
            var mask = new AllNodesMask();
            mask.IsShowMask = false;
            module.Push(mask);

            var presentation = SerializeDeserialize(myPresentation);

            module = presentation.GetModule<NodeMaskModule>();

            Assert.That(module, Is.Not.SameAs(myPresentation.GetModule<NodeMaskModule>()));
            Assert.That(module.Items.Single().IsShowMask, Is.False);
            Assert.That(module.Items.Single(), Is.InstanceOf<AllNodesMask>());
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
