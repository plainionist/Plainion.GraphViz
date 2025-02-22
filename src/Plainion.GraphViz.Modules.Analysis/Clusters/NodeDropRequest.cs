using Plainion.Windows.Interactivity.DragDrop;

namespace Plainion.GraphViz.Modules.Analysis.Clusters;

/// <summary>
/// Send as parameter with the <see cref="TreeEditor.DropCommand"/> to specify the requested DragDrop action.
/// </summary>
class NodeDropRequest
{
    public ClusterTreeNode DroppedNode { get; set; }

    public ClusterTreeNode DropTarget { get; set; }

    public DropLocation Location { get; set; }
}
