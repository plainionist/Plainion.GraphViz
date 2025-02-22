using Plainion.Windows.Interactivity.DragDrop;

namespace Plainion.GraphViz.Controls.Tree;

/// <summary>
/// Send as parameter with the <see cref="TreeEditor.DropCommand"/> to specify the requested DragDrop action.
/// </summary>
public class NodeDropRequest
{
    public INode DroppedNode { get; set; }

    public INode DropTarget { get; set; }

    public DropLocation Location { get; set; }
}
