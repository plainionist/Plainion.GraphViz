module Plainion.GraphViz.Help.Server

open System.Threading.Tasks

[<AutoOpen>]
module internal Impl =
    open Suave
    open Suave.Successful
    open Suave.Operators
    open Suave.Filters
    open System.Threading
    open System.Net

    let page (req:HttpRequest) =
        sprintf "Page: %s" req.path

    let myApp : WebPart =
        choose [ 
            GET >=> request (fun req -> OK (page req)) 
            Files.browseHome
        ]

    let mutable myCTS : CancellationTokenSource = null

    let start documentRoot =
        myCTS <- new CancellationTokenSource()

        // http://www.fssnip.net/7Po/title/Start-Suave-server-on-first-free-port

        Async.FromContinuations(fun (cont, _, _) ->
            let startedEvent = Event<_>()
            startedEvent.Publish.Add(cont)

            async {
                let rnd = System.Random()

                let rec tryStart () =
                    let port = 8000us + (uint16 (rnd.Next 2000))
                    let local = HttpBinding.create HTTP IPAddress.Loopback port
                    let config = { defaultConfig with homeFolder = Some documentRoot 
                                                      cancellationToken = myCTS.Token 
                                                      bindings = [ local ]
                                    }
                    let started, server = startWebServerAsync config myApp

                    async { 
                        let! running = started   
                        startedEvent.Trigger(port) 
                    } |> Async.Start                    

                    try 
                        Async.RunSynchronously(server, 0, myCTS.Token)
                    with 
                        :? System.Net.Sockets.SocketException -> tryStart ()
                
                tryStart () 
            } |> Async.Start 
        )

    let stop() =
        if myCTS <> null then
            myCTS.Cancel()
            myCTS <- null

let Start documentRoot = 
    Task.Run( fun () -> start documentRoot |> Async.RunSynchronously )

let Stop () = 
    stop()

