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

See also:

<iframe width="560" height="315" src="https://www.youtube.com/embed/SNk_a6A739I" title="YouTube video player" frameborder="0" allow="accelerometer; autoplay; clipboard-write; encrypted-media; gyroscope; picture-in-picture; web-share" allowfullscreen></iframe>

# Code Inspection Module

## Architecture

Code inspections use Akka.Net Remoting to outsource the actual analysis into another process which can be closed
to release the loaded assemblies. This process is the Plainion.GraphViz.Actors.Host.

Each code inspection is separated into the following components

- Analyzer: contains the actual inspection logic
- Actor: an Akka.Net actor hosting and executing the analyzer in the remote process
- Client: TPL based client side interface to the actor
- View/ViewModel: MVVM based UI for the code inspection

## How to implement a code inspection?

Follow this template to create a code inspection:

- implement the actual inspection logic in a class called "analyzer"
- derive a custom actor from the ActorsBase class
- derive a custom client rom the ActorClientBase
- implement view and viewmodel, the later uses the client to communicate with the actor
- create custom request and response messages
  - respone needs to derive from finish
- usually an inspection creates some kind of "result document". Usually these classes cannot easily be serialized
  by Akka.Net. Therefore use the DocumentSerializer to pre-serialize the document and then send the result with
  the "finished" message

## Where to do the threading?

As of now we use Akka.Net only for remoting - not for threading. For consistency reasons use TPL within the analyzer
for multi-threading.

Once we migrate to actors based multi-threading this guideline will change as well.
