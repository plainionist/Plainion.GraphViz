
# Plainion.GraphViz.Presentation.DynamicClusterTransformation

**Namespace:** Plainion.GraphViz.Presentation

**Assembly:** Plainion.GraphViz


## Constructors

### Constructor()


## Properties

### System.Collections.Generic.IReadOnlyDictionary`2[[System.String, mscorlib, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089],[System.String, mscorlib, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089]] NodeToClusterMapping

### System.Collections.Generic.IReadOnlyDictionary`2[[System.String, mscorlib, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089],[System.Boolean, mscorlib, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089]] ClusterVisibility


## Methods

### void AddCluster(System.String clusterId)

### void HideCluster(System.String clusterId)

### void ResetClusterVisibility(System.String clusterId)

### void AddToCluster(System.String nodeId,System.String clusterId)

### void AddToCluster(System.Collections.Generic.IReadOnlyCollection`1[System.String] nodeIds,System.String clusterId)

### void RemoveFromClusters(System.String[] nodeIds)

### Plainion.GraphViz.Model.IGraph Transform(Plainion.GraphViz.Model.IGraph graph)
