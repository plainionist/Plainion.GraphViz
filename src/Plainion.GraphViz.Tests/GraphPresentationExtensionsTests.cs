using System.Linq;
using NUnit.Framework;
using Plainion.Graphs;
using Plainion.Graphs.Projections;
using Plainion.GraphViz.Presentation;

namespace Plainion.GraphViz.Tests;

[TestFixture]
public class GraphPresentationExtensionsTests
{
    [TestCase(SiblingsType.Sources, "a")]
    [TestCase(SiblingsType.Targets, "c")]
    public void SelectNodesWithoutSiblings_AddsOnlyMatchingVisibleNodes(SiblingsType type, string matchingNode)
    {
        var builder = new RelaxedGraphBuilder();
        builder.TryAddEdge("a", "b");
        builder.TryAddEdge("b", "c");
        builder.TryAddEdge("c", "hidden");
        builder.TryAddEdge("hidden", "a");
        builder.TryAddNode("isolated");

        var presentation = new GraphPresentation(builder.Graph);
        var hiddenMask = new NodeMask(new[] { "hidden" }) { IsShowMask = false };
        presentation.Masks().Push(hiddenMask);

        var selection = presentation.GetPropertySetFor<Selection>();
        selection.Get("b").IsSelected = true;
        var selectedEdge = presentation.Graph.Edges.First(e => e.Source.Id == "a");
        selection.Get(selectedEdge.Id).IsSelected = true;

        presentation.SelectNodesWithoutSiblings(type);

        Assert.That(selection.Items.Where(s => s.IsSelected).Select(s => s.OwnerId),
            Is.EquivalentTo(new[] { matchingNode, "isolated", "b", selectedEdge.Id }));
        Assert.That(presentation.Masks().Items, Is.EqualTo(new[] { hiddenMask }));
    }
}
