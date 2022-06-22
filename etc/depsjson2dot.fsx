
#r "System.Core.dll"
#r "nuget: Newtonsoft.Json" 

//
// usage:
// when building a Asp.Net Core application a file called <app-name.deps.json>
// from "Developer Command prompt" run "fsi <full-path-to-this-script> <app-name.deps.json>"
// (if you don't have the "Developer Command prompt" use the full path to the F# installation)
// load the created file (location printed to console) into Plainion.GraphViz
//

open System.IO
open Newtonsoft.Json.Linq

let convert input (output:string) =
    let doc = JObject.Parse(File.ReadAllText(input))

    use writer = new StreamWriter(output)
    
    writer.WriteLine("digraph {")

    (doc.["targets"] :?> JObject).Properties()
    |> Seq.collect(fun target ->
        (target.Value :?> JObject).Properties()
        |> Seq.collect(fun p ->
            let deps = 
                match p.Value.["dependencies"] with
                | null -> []
                | deps ->
                    (deps :?> JObject).Properties()
                    |> Seq.map(fun d -> (p.Name, (sprintf "%s/%s" d.Name (d.Value.ToString()))))
                    |> List.ofSeq
            (target.Name, p.Name)::deps))
    |> Seq.distinct
    |> Seq.iter(fun (s,t) -> writer.WriteLine($"  \"{s}\" -> \"{t}\""))

    writer.WriteLine("}")

    ()


let depsJson = fsi.CommandLineArgs.[1]

let output = Path.Combine(Path.GetDirectoryName(depsJson),Path.GetFileNameWithoutExtension(depsJson) + ".dot")

convert depsJson output

printfn "Output written to: %s" output
