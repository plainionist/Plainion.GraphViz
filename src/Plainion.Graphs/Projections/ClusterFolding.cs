using System;
using System.Collections.Generic;
using System.Linq;

namespace Plainion.Graphs.Projections;

/// <summary>
/// Manages folding of clusters
/// </summary>
public class ClusterFolding : NotifyPropertyChangedBase, IClusterFolding
{
    private readonly IGraphProjections myProjections;

    public ClusterFolding(IGraphProjections projections)
    {
        Contract.RequiresNotNull(projections);

        myProjections = projections;

        FoldedClusters = [];
    }

    protected HashSet<string> FoldedClusters { get; }
    protected bool IsChangeNotified { get; set; }

    protected void NotifyTransformationHasChanged()
    {
        IsChangeNotified = true;
        OnPropertyChanged(nameof(Clusters));
    }

    public IReadOnlyCollection<string> Clusters
    {
        get { return FoldedClusters; }
    }

    public string GetClusterNodeId(string clusterId)
    {
        return "[" + clusterId + "]";
    }

    public virtual IReadOnlyCollection<Node> GetNodes(string clusterId) =>
        myProjections.Graph.Clusters.Single(c => c.Id == clusterId).Nodes;

    public void Add(string clusterId)
    {
        if (FoldedClusters.Contains(clusterId))
        {
            return;
        }

        AddInternal(clusterId);

        NotifyTransformationHasChanged();
    }

    protected virtual void AddInternal(string clusterId)
    {
        var clusterNodeId = GetClusterNodeId(clusterId);
        FoldedClusters.Add(clusterId);
    }

    public void Add(IEnumerable<string> clusterIds)
    {
        var clustersToAdd = clusterIds
            .Except(FoldedClusters)
            .ToList();

        if (clustersToAdd.Count == 0)
        {
            return;
        }

        foreach (var cluster in clustersToAdd)
        {
            AddInternal(cluster);
        }

        NotifyTransformationHasChanged();
    }

    public void Remove(string clusterId)
    {
        var removed = FoldedClusters.Remove(clusterId);

        if (removed)
        {
            NotifyTransformationHasChanged();
        }
    }

    public void Remove(IEnumerable<string> clusterIds)
    {
        var clustersToRemove = clusterIds
            .Intersect(FoldedClusters)
            .ToList();

        if (clustersToRemove.Count == 0)
        {
            return;
        }

        foreach (var cluster in clustersToRemove)
        {
            FoldedClusters.Remove(cluster);
        }

        NotifyTransformationHasChanged();
    }

    public void Toggle(string clusterId)
    {
        if (FoldedClusters.Contains(clusterId))
        {
            Remove(clusterId);
        }
        else
        {
            Add(clusterId);
        }
    }
}
