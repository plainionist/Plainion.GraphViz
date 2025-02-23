namespace Plainion.GraphViz.Modules.Analysis.Clusters;

class DragDropBehavior
{
    private readonly NodeViewModel myRoot;

    public DragDropBehavior(NodeViewModel root)
    {
        System.Contract.RequiresNotNull(root);
        myRoot = root;
    }

    public void ApplyDrop(NodeDropRequest request)
    {
        if (request.DropTarget == myRoot)
        {
            var oldParent = request.DroppedNode.Parent;
            oldParent.Children.Remove(request.DroppedNode);

            myRoot.Children.Add(request.DroppedNode);

            request.DroppedNode.Parent = myRoot;
        }
        else
        {
            var oldParent = request.DroppedNode.Parent;
            oldParent.Children.Remove(request.DroppedNode);

            request.DropTarget.Children.Add(request.DroppedNode);

            request.DroppedNode.Parent = request.DropTarget;
        }
    }
}
