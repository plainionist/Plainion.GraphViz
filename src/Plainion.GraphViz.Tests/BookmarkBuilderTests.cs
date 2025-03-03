using System.Linq;
using NUnit.Framework;
using Plainion.Graphs.Algorithms;
using Plainion.Graphs;
using Plainion.GraphViz.Presentation;
using Plainion.Graphs.Projections;

namespace Plainion.GraphViz.Tests
{
    [TestFixture]
    class BookmarkBuilderTests
    {
        private IGraphPresentation myPresentation;
        private BookmarkBuilder myBuilder;

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

            myBuilder = new BookmarkBuilder();
        }

        [Test]
        public void WHEN_NoFiltersNoFolding_THEN_NothingChanged()
        {
            var bookmark = myBuilder.Create(myPresentation, "test");
            Assert.That(bookmark, Is.Not.Null);

            myBuilder.Apply(myPresentation, bookmark);

            Assert.That(myPresentation.GetModule<NodeMaskModule>().Items, Is.Empty);

            // there is a default transformation always added
            Assert.That(myPresentation.GetModule<TransformationModule>().Items.Count(), Is.EqualTo(1));
        }

        [Test]
        public void WHEN_FiltersExist_THEN_FiltersRestored()
        {
            var nodeC = myPresentation.Graph.Nodes.Single(n => n.Id == "c");
            var module = myPresentation.GetModule<NodeMaskModule>();

            var mask = new NodeMask();
            mask.Set(nodeC);
            module.Push(mask);

            var bookmark = myBuilder.Create(myPresentation, "test");
            Assert.That(bookmark, Is.Not.Null);

            myBuilder.Apply(myPresentation, bookmark);

            Assert.That(module.Items.Count(), Is.EqualTo(1));

            var restoredMask = module.Items.Single();

            Assert.That(restoredMask, Is.Not.SameAs(mask));
            Assert.That(restoredMask.IsSet(nodeC), Is.True);
        }

        [Test]
        public void WHEN_FoldingExist_THEN_FoldingRestored()
        {
            var nodeC = myPresentation.Graph.Nodes.Single(n => n.Id == "c");
            var module = myPresentation.GetModule<TransformationModule>();

            myPresentation.ClusterFolding.Toggle("X17");

            Assert.That(module.Items.OfType<ClusterFoldingTransformation>().Single().Clusters, Is.EquivalentTo(new[] { "X17" }));

            var bookmark = myBuilder.Create(myPresentation, "test");
            Assert.That(bookmark, Is.Not.Null);

            // unfold again to be sure that folding comes really from bookmark
            myPresentation.ClusterFolding.Toggle("X17");

            myBuilder.Apply(myPresentation, bookmark);

            Assert.That(module.Items.OfType<ClusterFoldingTransformation>().Single().Clusters, Is.EquivalentTo(new[] { "X17" }));
        }
    }
}
