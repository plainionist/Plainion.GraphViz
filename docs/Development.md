---
title: Contributing &amp; Extending
navigation_weight: 1
---

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

