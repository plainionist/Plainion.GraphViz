using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Plainion.GraphViz.Pioneer.Services;
using Plainion.GraphViz.Pioneer.Spec;

namespace Plainion.GraphViz.Pioneer.Activities
{
    class AnalyzePackageDependencies : AnalyzeBase
    {
        private static string[] Colors = { "lightblue", "lightgreen", "lightgray" };

        private readonly Dictionary<string, List<Type>> myPackages = new Dictionary<string, List<Type>>();

        protected override void Execute()
        {
            foreach (var package in Config.Packages)
            {
                myPackages[package.Name] = Load(package)
                    .SelectMany(asm => asm.GetTypes())
                    .ToList();
            }

            Console.WriteLine("Analyzing ...");

            var tasks = Config.Packages
                .Select(p => Task.Run<Tuple<Type, Type>[]>(() => Analyze(p)))
                .ToArray();

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

            var output = Path.GetFullPath("packaging.dot");
            Console.WriteLine("Output: {0}", output);

            using (var writer = new StreamWriter(output))
            {
                writer.WriteLine("digraph {");

                for (int i = 0; i < Config.Packages.Count; ++i)
                {
                    var package = Config.Packages[i];

                    var clusters = new Dictionary<string, List<string>>();

                    foreach (var cluster in package.Clusters)
                    {
                        clusters[cluster.Name] = new List<string>();
                    }

                    foreach (var node in myPackages[package.Name].Select(GraphUtils.Node).Distinct())
                    {
                        if (!edges.Any(e => e.Item1 == node || e.Item2 == node))
                        {
                            continue;
                        }

                        var nodeDesc = string.Format("  \"{0}\" [color = {1}, label = {2}]", node.FullName, Colors[i], node.Name);

                        // in case multiple cluster match we just take the first one
                        var matchedCluster = package.Clusters.FirstOrDefault(c => c.Matches(node.FullName));
                        if (matchedCluster != null)
                        {
                            clusters[matchedCluster.Name].Add(nodeDesc);
                        }
                        else
                        {
                            writer.WriteLine(nodeDesc);
                        }
                    }

                    foreach (var entry in clusters.Where(e => e.Value.Any()))
                    {
                        writer.WriteLine();
                        writer.WriteLine("  subgraph " + entry.Key + " {");

                        foreach (var nodeDesc in entry.Value)
                        {
                            writer.WriteLine("  " + nodeDesc);
                        }

                        writer.WriteLine("  }");
                        writer.WriteLine();
                    }
                }

                writer.WriteLine();

                foreach (var edge in edges)
                {
                    writer.WriteLine("  \"{0}\" -> \"{1}\"", edge.Item1.FullName, edge.Item2.FullName);
                }

                writer.WriteLine("}");
            }
        }

        private Tuple<Type, Type>[] Analyze(Package p)
        {
            var tasks = myPackages[p.Name]
                .Select(t => Task.Run<IEnumerable<Tuple<Type, Type>>>(() => Analyze(p, t)))
                .ToArray();

            Task.WaitAll(tasks);

            return tasks.SelectMany(t => t.Result).ToArray();
        }

        private IEnumerable<Tuple<Type, Type>> Analyze(Package package, Type type)
        {
            Console.Write(".");

            return new Reflector(AssemblyLoader, type).GetUsedTypes()
                .Where(usedType => IsForeignPackage(package, usedType))
                .Select(usedType => GraphUtils.Edge(type, usedType));
        }

        private bool IsForeignPackage(Package package, Type dep)
        {
            return myPackages.Where(e => e.Key != package.Name).Any(entry => entry.Value.Contains(dep));
        }
    }
}
