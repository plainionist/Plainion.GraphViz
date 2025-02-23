using Plainion.Windows.Interactivity.DragDrop;

namespace Plainion.GraphViz.Modules.Analysis.Clusters;

class NodeDropRequest
{
    public NodeViewModel DroppedNode { get; set; }

    public NodeViewModel DropTarget { get; set; }

    public DropLocation Location { get; set; }
}
