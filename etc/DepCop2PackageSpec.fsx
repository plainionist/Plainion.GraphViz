#r "nuget: FSharp.Data"

open System
open System.IO
open System.Xml.Linq
open FSharp.Data

type RulesXml = XmlProvider<"DepCopRules.xml">

let rulesFile = fsi.CommandLineArgs.[1] 

let rules = rulesFile |> RulesXml.Load

let createPattern (rule:RulesXml.Rule) =
    match rule.Class with
    | Some x -> x
    | None -> rule.Namespace.Value + "*"

let equalsI (lhs:string) rhs = lhs.Equals(rhs, StringComparison.OrdinalIgnoreCase)

let ns = XNamespace.Get "http://github.com/ronin4net/plainion/GraphViz/Packaging/Spec"

let createClusters() = seq {
        yield! rules.Rules
            |> Seq.filter(fun x -> equalsI x.Category "leaf" || equalsI x.Category "shared")
            |> Seq.map(fun x -> XElement(ns + "Cluster",
                XAttribute("Name", x.Namespace.Value),
                XElement(ns + "Include", XAttribute("Pattern", createPattern x))))

        yield XElement(ns + "Cluster",
                XAttribute("Name", "Composition"),
                rules.Rules
                |> Seq.filter(fun x -> equalsI x.Category "composition")
                |> Seq.map(fun x -> XElement(ns + "Include", XAttribute("Pattern", createPattern x))))
    }

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

let outputFile = Path.Combine(Path.GetDirectoryName rulesFile, Path.GetFileNameWithoutExtension rulesFile + ".xaml")
File.WriteAllText(outputFile, root.ToString())

printfn "Packaging spec written to: %s" outputFile
