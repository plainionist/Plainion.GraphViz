---
title: Contributing &amp; Extending
section: Contribution
navigation_weight: 200
---

# Development

Plainion.GraphViz is developed based on the .NET 8.0 in C#, so you need to install .NET 8.0 SDK.

All other dependencies are handled via NuGet.

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

# Algorithm Design

- Algorithms do not modify the presentation directly
- "Add" and "Remove" algorithms generate delta masks which contain the delta from currently
  visible graph to the desired result.
- "Add" algorithms only consider nodes not being visible.
- "Remove" algorithms only consider nodes being visible.
- In order to generate "Show" masks visualizing a certain sub graph an algorithm has two options:
  - generate two masks: one "hide mask" containing all visible nodes and
    a "show mask" containing the desired nodes
  - generate an inverted "hide mask" containing the desired nodes

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
