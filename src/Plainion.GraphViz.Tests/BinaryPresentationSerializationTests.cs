using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows.Media;
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

        [Test]
        public void WHEN_ClusterVisibilityIsDefined_THEN_ClusterVisibilityIsSerialized()
        {
            var module = myPresentation.GetModule<TransformationModule>();
            var t = module.Items.OfType<DynamicClusterTransformation>().Single();
            t.HideCluster("X17");

            var presentation = SerializeDeserialize(myPresentation);

            module = presentation.GetModule<TransformationModule>();

            Assert.That(module, Is.Not.SameAs(myPresentation.GetModule<TransformationModule>()));

            t = module.Items.OfType<DynamicClusterTransformation>().Single();
            Assert.That(t.ClusterVisibility["X17"], Is.False);
        }

        [Test]
        public void WHEN_NodeAddedToCluster_THEN_ClusterAssignmentIsSerialized()
        {
            var module = myPresentation.GetModule<TransformationModule>();
            var t = module.Items.OfType<DynamicClusterTransformation>().Single();
            t.AddToCluster("d", "X17");

            var presentation = SerializeDeserialize(myPresentation);

            module = presentation.GetModule<TransformationModule>();

            Assert.That(module, Is.Not.SameAs(myPresentation.GetModule<TransformationModule>()));

            t = module.Items.OfType<DynamicClusterTransformation>().Single();
            Assert.That(t.NodeToClusterMapping["d"], Is.EqualTo("X17"));
        }

        [Test]
        public void WHEN_ClusterFolded_THEN_ClusterFoldingSerialized()
        {
            var module = myPresentation.GetModule<TransformationModule>();
            var t = new ClusterFoldingTransformation(myPresentation);
            t.Add("X17");
            module.Add(t);

            var presentation = SerializeDeserialize(myPresentation);

            module = presentation.GetModule<TransformationModule>();

            Assert.That(module, Is.Not.SameAs(myPresentation.GetModule<TransformationModule>()));

            t = module.Items.OfType<ClusterFoldingTransformation>().Single();
            Assert.That(t.Clusters, Is.EquivalentTo(new[] { "X17" }));
        }

        [Test]
        public void WHEN_CaptionsDefined_THEN_CaptionsAreSerialized()
        {
            var module = myPresentation.GetModule<CaptionModule>();
            module.Add(new Caption("a", "this is an A"));

            var presentation = SerializeDeserialize(myPresentation);

            module = presentation.GetModule<CaptionModule>();

            Assert.That(module, Is.Not.SameAs(myPresentation.GetModule<TransformationModule>()));
            Assert.That(module.Get("a").Label, Is.EqualTo("this is an A"));
        }

        [Test]
        public void WHEN_NodeStylesDefined_THEN_NodeStylesAreSerialized()
        {
            var module = myPresentation.GetPropertySetFor<NodeStyle>();
            module.Add(new NodeStyle("a") { FillColor = Brushes.Red });

            var presentation = SerializeDeserialize(myPresentation);

            module = presentation.GetPropertySetFor<NodeStyle>();

            Assert.That(module, Is.Not.SameAs(myPresentation.GetPropertySetFor<NodeStyle>()));
            var converter = new BrushConverter();
            Assert.That(converter.ConvertToString(module.Get("a").FillColor), Is.EqualTo(converter.ConvertToString(Brushes.Red)));
        }

        [Test]
        public void WHEN_EdgeStylesDefined_THEN_EdgeStylesAreSerialized()
        {
            var module = myPresentation.GetPropertySetFor<EdgeStyle>();
            module.Add(new EdgeStyle("a") { Color = Brushes.Red });

            var presentation = SerializeDeserialize(myPresentation);

            module = presentation.GetPropertySetFor<EdgeStyle>();

            Assert.That(module, Is.Not.SameAs(myPresentation.GetPropertySetFor<EdgeStyle>()));
            var converter = new BrushConverter();
            Assert.That(converter.ConvertToString(module.Get("a").Color), Is.EqualTo(converter.ConvertToString(Brushes.Red)));
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
