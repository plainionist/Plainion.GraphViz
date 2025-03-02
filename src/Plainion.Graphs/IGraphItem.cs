namespace Plainion.Graphs;

/// <summary>
/// GraphItem are NOT equal just because the ID is equal. Examples: folding and handling only visible edges in folding
/// -> do NOT implement IEquatable
/// </summary>
public interface IGraphItem
{
    string Id { get; }
}
