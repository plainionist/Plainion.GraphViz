using Plainion.Graphs;

namespace Plainion.GraphViz.Viewer.Abstractions.ViewModel
{
    public class NodeWithCaption
    {
        public NodeWithCaption(Node node, string displayText)
        {
            Node = node;
            DisplayText = displayText;
        }

        public Node Node { get; private set; }
        public string DisplayText { get; private set; }
    }
}
