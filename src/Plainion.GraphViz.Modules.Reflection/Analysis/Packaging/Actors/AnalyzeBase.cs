using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Plainion.GraphViz.Model;
using Plainion.GraphViz.Modules.Reflection.Analysis.Packaging;
using Plainion.GraphViz.Modules.Reflection.Analysis.Packaging.Spec;
using Plainion.GraphViz.Presentation;

namespace Plainion.GraphViz.Modules.Reflection.Analysis.Packaging.Actors
{
    abstract class AnalyzeBase
    {
        protected AnalyzeBase()
        {
            AssemblyLoader = new AssemblyLoader();
        }

        protected SystemPackaging Config { get; private set; }

        protected AssemblyLoader AssemblyLoader { get; private set; }

        public void Execute(SystemPackaging config)
        {
            Config = config;

            Load();

            Console.WriteLine("Analyzing ...");

            var tasks = Analyze();

            Task.WaitAll(tasks);

            Console.WriteLine();

            if (AssemblyLoader.SkippedAssemblies.Any())
            {
                Console.WriteLine("Skipped assemblies:");
                foreach (var asm in AssemblyLoader.SkippedAssemblies)
                {
                    Console.WriteLine("  {0}", asm);
                }
                Console.WriteLine();
            }

            var edges = tasks
                .SelectMany(t => t.Result)
                .Distinct()
                .ToList();

            var doc = GenerateDocument(edges);

            WriteDocument(doc);
        }

        protected abstract void Load();

        protected abstract Task<Tuple<Type, Type>[]>[] Analyze();

        protected abstract AnalysisDocument GenerateDocument(IReadOnlyCollection<Tuple<Type, Type>> edges);

        private void WriteDocument(AnalysisDocument document)
        {
            Console.WriteLine("Output: {0}", OutputFile);

            using (var writer = new StreamWriter(OutputFile))
            {
                writer.WriteLine("digraph {");

                Func<Node, string> GetNodeDef = node =>
                {
                    var style = document.NodeStyles.SingleOrDefault(n => n.OwnerId == node.Id) ?? new NodeStyle(node.Id);
                    var caption = document.Captions.SingleOrDefault(n => n.OwnerId == node.Id) ?? new Caption(node.Id, null);
                    return string.Format("\"{0}\" [color = {1}, label = {2}]", node.Id, style.FillColor.ToString(), caption.DisplayText);
                };

                foreach (var node in document.Graph.Nodes.Where(n => !document.Graph.Clusters.Any(c => c.Nodes.Contains(n))))
                {
                    writer.WriteLine("  " + GetNodeDef(node));
                }

                foreach (var cluster in document.Graph.Clusters)
                {
                    writer.WriteLine();
                    writer.WriteLine("  subgraph " + cluster.Id + " {");

                    foreach (var node in cluster.Nodes)
                    {
                        writer.WriteLine("    " + GetNodeDef(node));
                    }

                    writer.WriteLine("  }");
                    writer.WriteLine();
                }

                writer.WriteLine();

                foreach (var edge in document.Graph.Edges)
                {
                    writer.Write("  \"{0}\" -> \"{1}\"", edge.Source.Id, edge.Target.Id);

                    var attributes = new List<string>();

                    var style = document.EdgeStyles.SingleOrDefault(e => e.OwnerId == edge.Id);
                    if (style != null)
                    {
                        attributes.Add("color = " + style.Color);
                    }

                    var caption = document.Captions.SingleOrDefault(e => e.OwnerId == edge.Id);
                    if (caption != null)
                    {
                        attributes.Add(string.Format("label = \"{0}\"", caption.DisplayText));
                    }

                    if (attributes.Count == 0)
                    {
                        writer.WriteLine();
                    }
                    else
                    {
                        writer.WriteLine(string.Format("[{0}]", string.Join(",", attributes)));
                    }
                }

                writer.WriteLine("}");
            }
        }

        protected IEnumerable<Assembly> Load(Package package)
        {
            Console.WriteLine("Loading package {0}", package.Name);

            return package.Includes
                .SelectMany(i => Directory.GetFiles(Config.AssemblyRoot, i.Pattern))
                .Where(file => !package.Excludes.Any(e => e.Matches(file)))
                .Select(AssemblyLoader.Load)
                .Where(asm => asm != null)
                .ToList();
        }

        public string OutputFile { get; set; }
    }
}
