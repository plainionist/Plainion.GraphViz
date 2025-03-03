namespace Plainion.Graphs.Projections;

public interface IGraphProjections
{
    IGraph Graph { get; }
    IGraph TransformedGraph { get; }

    IGraphPicking Picking { get; }
    IClusterFolding ClusterFolding { get; }

    string GetCaption(string id);
}
