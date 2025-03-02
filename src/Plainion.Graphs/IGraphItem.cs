namespace Plainion.GraphViz.Model
{
    // GraphItem are NOT equal just because the ID is equal. Examples: folding and handling only visible edges in folding
    // -> do NOT implement IEquatable
    public interface IGraphItem 
    {
        string Id { get; }
    }
}
