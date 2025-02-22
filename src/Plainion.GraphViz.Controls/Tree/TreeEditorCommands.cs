using System.Windows.Input;

namespace Plainion.GraphViz.Controls.Tree;

/// <summary>
/// Defaults for the <see cref="TreeEditor"/> commands.
/// </summary>
public class TreeEditorCommands
{
    public static readonly RoutedCommand ExpandAll = new RoutedCommand();
    public static readonly RoutedCommand CollapseAll = new RoutedCommand();

    public static void RegisterCommandBindings(TreeEditor editor)
    {
        editor.CommandBindings.Add(new CommandBinding(ExpandAll, (sender, e) => OnExpandAll(editor, (INode)e.Parameter)));
        editor.CommandBindings.Add(new CommandBinding(CollapseAll, (sender, e) => OnCollapseAll(editor, (INode)e.Parameter)));
    }

    private static void OnExpandAll(TreeEditor editor, INode node)
    {
        if (node == null)
        {
            node = editor.Root;
        }

        var nodeState = editor.myTree.StateContainer.GetOrCreate(node);

        nodeState.ExpandAll();
    }

    private static void OnCollapseAll(TreeEditor editor, INode node)
    {
        if (node == null)
        {
            node = editor.Root;
        }

        var nodeState = editor.myTree.StateContainer.GetOrCreate(node);

        nodeState.CollapseAll();
    }
}
