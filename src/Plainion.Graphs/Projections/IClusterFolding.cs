using System.Collections.Generic;

namespace Plainion.Graphs.Projections;

public interface IClusterFolding
{
    IReadOnlyCollection<string> Clusters { get; }
    
    IReadOnlyCollection<Node> GetNodes(string clusterId);

    void Add(string clusterId);
    void Add(IEnumerable<string> clusterIds);
    void Remove(string clusterId);
    void Remove(IEnumerable<string> clusterIds);

    void Toggle(string clusterId);
}
