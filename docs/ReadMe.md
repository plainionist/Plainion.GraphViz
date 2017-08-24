
Plainion.GraphViz is a browser for complex and huge graphs. 

It makes it easy to dive into any kind of graph from all different kind of sources by interactively 
allowing to show, hide and fold any part of the graph.

![](Screenshots/Overview.png)

## Installation

- download [latest release](https://github.com/plainionist/Plainion.GraphViz/releases) and unpack it somewhere
- start the Plainion.GraphViz.Viewer.exe

## Usage

Once a graph was imported (see below)

- use search edit box to fast navigate to nodes 
- use mouse wheel and right mouse button drag for zoom 
- use left mouse button for pan 
- use context menu on nodes, edges and clusters to morph the graph into any shape 
- use ![](Screenshots/Filter.png) to filter the graph based on regex
- use ![](Screenshots/Clusters.png) to define clusters with Drag&Drop

### Importing graphs from documents

Use the "Open" button from the toolbar to load graphs from documents. 
The following formats are supported:

- GraphML
- DGML
- DOT

Try out the [samples](Viewer.Samples/).

If the document gets modified while loaded into Plainion.GraphViz the graph will automatically updated.

### Importing graphs from source code

Use the "Tools" button from the toolbar to load graphs from other "sources".
The following tools are supported:

- Generate graphs from inheritance hierarchies (.Net only)
- Generate graphs from software packages or sub-systems (.Net only)
  (see [Packaging Sample](Viewer.Samples/Packaging.xaml))

Which kind of galaxy does your code form?

![](Screenshots/Galaxy.1.png)

![](Screenshots/Galaxy.2.png)

