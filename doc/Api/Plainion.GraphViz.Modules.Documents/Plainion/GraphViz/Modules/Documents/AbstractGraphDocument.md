
# Plainion.GraphViz.Modules.Documents.AbstractGraphDocument

**Namespace:** Plainion.GraphViz.Modules.Documents

**Assembly:** Plainion.GraphViz.Modules.Documents


## Constructors

### Constructor()


## Properties

### Plainion.GraphViz.Model.IGraph Graph

### System.String Filename

### System.Collections.Generic.IEnumerable`1[[Plainion.GraphViz.Infrastructure.FailedItem, Plainion.GraphViz.Infrastructure, Version=1.18.0.0, Culture=neutral, PublicKeyToken=null]] FailedItems


## Methods

### void Load(System.String path)

### void Load()

### Plainion.GraphViz.Model.Node TryAddNode(System.String nodeId)

### Plainion.GraphViz.Model.Edge TryAddEdge(System.String sourceNodeId,System.String targetNodeId)

### Plainion.GraphViz.Model.Cluster TryAddCluster(System.String clusterId,System.Collections.Generic.IEnumerable`1[System.String] nodes)
