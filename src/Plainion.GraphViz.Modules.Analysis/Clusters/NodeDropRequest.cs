namespace Plainion.GraphViz.Modules.Analysis.Clusters;

class NodeDropRequest
{
    public NodeDropRequest(NodeViewModel droppedNode, NodeViewModel dropTarget)
    {
        System.Contract.RequiresNotNull(droppedNode);
        System.Contract.RequiresNotNull(dropTarget);

        DroppedNode = droppedNode;
        DropTarget = dropTarget;
    }

    public NodeViewModel DroppedNode { get; }
    public NodeViewModel DropTarget { get; }
}
