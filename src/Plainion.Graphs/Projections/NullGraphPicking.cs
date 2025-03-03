namespace Plainion.Graphs.Projections;

/// <summary>
/// Null-object pattern for <see cref="IGraphPicking"/>.
/// </summary>
public class NullGraphPicking : IGraphPicking
{
    public bool Pick(Node node) => true;
    public bool Pick(Edge edge) => true;
    public bool Pick(Cluster cluster) => true;
}
