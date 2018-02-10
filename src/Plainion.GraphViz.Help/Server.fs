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
    open System
    open Suave.Redirection

    let getTemplate documentRoot =
        let templateFile = Path.Combine(documentRoot, "Template.html")
        if File.Exists templateFile then
            File.ReadAllText templateFile
        else
            "@@@content@@@"

    let getNavigation documentRoot (template:string) =
        let getSectionName (folder:string) =
            sprintf "### %s" (folder.Replace("/", " / "))

        let getPageLink (file:string) =
            let path = file.Replace('\\','/')
            let name = Path.GetFileNameWithoutExtension file
            sprintf " - [%s](/%s)" name path

        let pageSorter (file:string) =
            if file.EndsWith("ReadMe.md", StringComparison.OrdinalIgnoreCase) then
                "0"
            else
                file

        if template.Contains("@@@navigation@@@") then
            Directory.GetFiles(documentRoot, "*.md", SearchOption.AllDirectories)
            |> Seq.map(fun p -> p.Substring(documentRoot.Length))
            |> Seq.map(fun p -> p.TrimStart('\\'))
            |> Seq.groupBy(fun p -> Path.GetDirectoryName(p).Replace('\\','/'))
            |> Seq.sortBy(fun (k,v) -> k)
            |> Seq.map(fun (folder, files) -> seq { yield getSectionName folder
                                                    yield Environment.NewLine
                                                    yield! files 
                                                            |> Seq.sortBy pageSorter
                                                            |> Seq.map getPageLink } )
            |> Seq.concat
            |> String.concat Environment.NewLine
            |> Markdown.ToHtml
        else
            ""

    let applyTemplate (template:string) navigationHtml contentHtml =
        template.Replace("@@@navigation@@@", navigationHtml).Replace("@@@content@@@", contentHtml)

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

        let documentRoot = Path.GetFullPath(documentRoot)

        let template = getTemplate documentRoot
        let navigation = getNavigation documentRoot template
        let applyTemplate = applyTemplate template navigation
        let page = page documentRoot >> applyTemplate

        let app : WebPart =
            choose [ 
                path "/" >=> redirect "/ReadMe"
                pathScan "/%s" (fun p -> OK (page (p + ".md")))
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

