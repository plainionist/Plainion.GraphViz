#r "nuget: FSharp.Data"

open System.IO
open System.Xml.Linq
open FSharp.Data

type RulesXml = XmlProvider<"config.nsdepcop">

let rulesFile = fsi.CommandLineArgs.[1] 

let rules = rulesFile |> RulesXml.Load

let ns = XNamespace.Get "http://github.com/ronin4net/plainion/GraphViz/Packaging/Spec"

let trimWildcards (s:string) = s.TrimEnd('*').TrimEnd('.')

// Idea: the "to" attribute of both the "allowed" rules and the "disallowed" rules
// are precise enough to specify "architectural building blocks"
let createClusters() = 
    rules.Alloweds
    |> Seq.map(fun x -> x.To |> trimWildcards)
    |> Seq.append (rules.Disalloweds |> Seq.map(fun x -> x.To |> trimWildcards))
    |> Seq.groupBy id
    |> Seq.distinct
    |> Seq.map(fun (name,rules) -> XElement(ns + "Cluster", seq {
            yield XAttribute("Name", name) :> XObject
            yield! rules |> Seq.map(fun x -> XElement(ns + "Include", XAttribute("Pattern", x + "*"))) |> Seq.cast<XObject>
        } |> Array.ofSeq))

let root = XElement(ns + "SystemPackaging",
    XAttribute("AssemblyRoot", "ASSEMBLY_ROOT_UNDEFINED"),
    XAttribute("NetFramework", "false"),
    XAttribute("UsedTypesOnly", "true"),
    XElement(ns + "Package",
        XAttribute("Name", "System"),
        XElement(ns + "Package.Clusters", createClusters() ),
        XElement(ns + "Include", XAttribute("Pattern", "*.dll"))
    )
)

let outputFile = rulesFile + ".xaml"
File.WriteAllText(outputFile, root.ToString())

printfn "Packaging spec written to: %s" outputFile
