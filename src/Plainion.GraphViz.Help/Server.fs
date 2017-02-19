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
    open System.IO
    open Markdig

    let addTemplate templateFile html =
        let insertContent (template:string) = template.Replace("@@@content@@@", html)

        if File.Exists templateFile then
            templateFile 
            |> File.ReadAllText
            |> insertContent
        else
            html

    let homePage documentRoot =
        "# Welcome to the online help!"
        |> Markdown.ToHtml

    let page documentRoot path =
        let localFile = Path.Combine(documentRoot, path)
        if File.Exists localFile then 
            localFile
            |> File.ReadAllText 
            |> Markdown.ToHtml
        else
            sprintf "No such page: %s" path

    let mutable myCTS : CancellationTokenSource = null

    let start documentRoot =
        myCTS <- new CancellationTokenSource()

        let templateFile = Path.Combine(documentRoot, "Template.html")
        let page = page documentRoot >> addTemplate templateFile
        let home () = documentRoot |> (homePage >> addTemplate templateFile)

        let app : WebPart =
            choose [ 
                path "/" >=> OK (home ()) 
                pathScan "/%s.md" (fun p -> OK (page (p + ".md")))
                Files.browseHome
            ]

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
                    let started, server = startWebServerAsync config app

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

