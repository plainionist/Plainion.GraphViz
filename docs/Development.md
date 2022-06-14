---
title: Contributing &amp; Extending
section: Contribution
navigation_weight: 200
---

# Development

Plainion.GraphViz is developed based on the .NET framework 4.7, mostly in C#.

As some dependencies of this project already reference .NET Core 2.0 assemblies you also need to install .Net Core 2.0 SDK.
See also [Announcing .Net Core 2.0](https://blogs.msdn.microsoft.com/dotnet/2017/08/14/announcing-net-core-2-0/).

All other dependencies are handled via NuGet.

Tests are written in [NUnit](http://nunit.org/). I recommend [NUnit 3 Test Adapter](https://marketplace.visualstudio.com/items?itemName=NUnitDevelopers.NUnit3TestAdapter)
for Visual Studio to run the tests. Of course you can also run the tests with NUnit runners directly.

# Extensibility

Plainion.GraphViz supports writing custom plug-ins to create graphs from custom sources.

Just download the source and have a look at Plainion.GraphViz.Modules.Documents and Plainion.GraphViz.Modules.CodeInspection
to see how to integrate plug-ins into Plainion.GraphViz. Inside the custom plug-in just

- parse the custom source (code, database, ...)
- generate a graph
- add optional meta information (e.g. edge colors, tooltips)
- pass it to the viewer

Plug-in assembly names need to match the following pattern: "Plainion.GraphViz.Modules.*.dll" and need to be placed
into the Plainion.GraphViz installation folder.
