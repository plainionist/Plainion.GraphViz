namespace Plainion.GraphViz.Modules.Analysis.Clusters;

/// <summary>
/// Can be implemented by <see cref="INode"/> implementations to control drag and drop allowence.
/// </summary>
public interface IDragDropSupport
{
    bool IsDragAllowed { get; }

    bool IsDropAllowed { get; }
}
