using Plainion.Windows.Interactivity.DragDrop;

namespace Plainion.GraphViz.Modules.Analysis.Clusters;

class NodeDropRequest
{
    public NodeDropRequest(NodeViewModel droppedNode, NodeViewModel dropTarget, DropLocation location)
    {
        System.Contract.RequiresNotNull(droppedNode);
        System.Contract.RequiresNotNull(dropTarget);

        DroppedNode = droppedNode;
        DropTarget = dropTarget;
        Location = location;
    }

    public NodeViewModel DroppedNode { get; }
    public NodeViewModel DropTarget { get; }
    public DropLocation Location { get; }
}
