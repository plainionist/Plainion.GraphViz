 
# Architecture

Code inspections use Akka.Net Remoting to outsource the actual analysis into another process which can be closed
to release the loaded assemblies. This process is the Plainion.GraphViz.Actors.Host.

Each code inspection is separated into the following components

- Analyzer: contains the actual inspection logic
- Actor: an Akka.Net actor hosting and executing the analyzer in the remote process
- Client: TPL based client side interface to the actor
- View/ViewModel: MVVM based UI for the code inspection

# How to implement a code inspection?

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

# Where to do the threading?

As of now we use Akka.Net only for remoting - not for threading. For consistency reasons use TPL within the analyzer
for multi-threading.

Once we migrate to actors based multi-threading this guideline will change as well.
