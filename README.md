
Plainion.GraphViz is a browser for complex and huge graphs. 

It makes it easy to dive into any kind of graph from all different kind of sources by interactively 
allowing to show, hide and fold any part of the graph.

![](docs/Screenshots/Overview.png)

## Installation

- download [latest release](https://github.com/plainionist/Plainion.GraphViz/releases) and unpack it somewhere
- start the Plainion.GraphViz.Viewer.exe

## Usage

Once a graph was imported (see below)

- use search edit box to fast navigate to nodes 
- use mouse wheel and right mouse button drag for zoom 
- use left mouse button for pan 
- use context menu on nodes, edges and clusters to morph the graph into any shape 
- use ![](docs/Screenshots/Filter.png) to filter the graph based on regex
- use ![](docs/Screenshots/Clusters.png) to define clusters with Drag&Drop

### Importing graphs from documents

Use the "Open" button from the toolbar to load graphs from documents. 
The following formats are supported:

- GraphML
- DGML
- DOT

Try out the [samples](docs/Viewer.Samples/).

If the document gets modified while loaded into Plainion.GraphViz the graph will automatically updated.

### Importing graphs from source code

Use the "Tools" button from the toolbar to load graphs from other "sources".
The following tools are supported:

- Generate graphs from inheritance hierarchies (.Net only)
- Generate graphs from software packages or sub-systems (.Net only)
  (see [Packaging Sample](docs/Viewer.Samples/Packaging.xaml))

Which kind of galaxy does your code form?

![](docs/Screenshots/Galaxy.1.png)

![](docs/Screenshots/Galaxy.2.png)

# Extensibility

Plainion.GraphViz supports writing custom plugins to create graphs from custom sources.

Just download the source and have a look at Plainion.GraphViz.Modules.Documents and Plainion.GraphViz.Modules.CodeInspection
to see how to integrate plugins into Plainion.GraphViz. Inside the custom plugin just

- parse the custom source (code, database, ...)
- generate a graph
- add optional meta information (e.g. edge colors, tooltips)
- pass it to the viewer

# Development

As some dependencies of this project already reference .NET Core 2.0 assemblies you need latest version of VS 2017 and .Net Core 2.0 SDK installed 
if you want to work with the code of this project.

(see also https://blogs.msdn.microsoft.com/dotnet/2017/08/14/announcing-net-core-2-0/)

