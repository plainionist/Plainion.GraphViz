using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Plainion.GraphViz.Pioneer.Services;
using Plainion.GraphViz.Pioneer.Spec;

namespace Plainion.GraphViz.Pioneer.Activities
{
    class AnalyzeSubSystemDependencies
    {
        private Package myPackage;
        private readonly List<Type> myTypes = new List<Type>();
        private readonly AssemblyLoader myLoader = new AssemblyLoader();

        internal void Execute(Config config, string packageName)
        {
            myPackage = config.Packages.Single(p => p.Name.Equals(packageName, StringComparison.OrdinalIgnoreCase));

            Console.WriteLine("Loading package {0}", myPackage.Name);

            var assemblies = myLoader.Load(config.AssemblyRoot, myPackage);

            myTypes.AddRange(assemblies.SelectMany(asm => asm.GetTypes()));

            Console.WriteLine("Analyzing ...");

            //Debugger.Launch();

            var tasks = config.Packages
                .Select(p => Task.Run<Tuple<Type, Type>[]>(() => Analyze()))
                .ToArray();

            Task.WaitAll(tasks);

            Console.WriteLine();

            if (myLoader.SkippedAssemblies.Any())
            {
                Console.WriteLine("Skipped assemblies:");
                foreach (var asm in myLoader.SkippedAssemblies)
                {
                    Console.WriteLine("  {0}", asm);
                }
                Console.WriteLine();
            }

            var edges = tasks
                .SelectMany(t => t.Result)
                .Distinct()
                .ToList();

            DrawGraph(edges);
        }

        private Tuple<Type, Type>[] Analyze()
        {
            var tasks = myTypes
                .Select(t => Task.Run<IEnumerable<Tuple<Type, Type>>>(() => Analyze(t)))
                .ToArray();

            Task.WaitAll(tasks);

            return tasks.SelectMany(t => t.Result).ToArray();
        }

        private IEnumerable<Tuple<Type, Type>> Analyze(Type type)
        {
            Console.Write(".");

            var cluster = myPackage.Clusters.FirstOrDefault(c => c.Matches(type.FullName));

            return new Reflector(myLoader, type).GetUsedTypes()
                .Where(myTypes.Contains)
                .Where(t => type != t)
                .Where(t => cluster == null || cluster != myPackage.Clusters.FirstOrDefault(c => c.Matches(t.FullName)))
                .Select(usedType => GraphUtils.Edge(type, usedType));
        }

        private void DrawGraph(List<Tuple<Type, Type>> edges)
        {
            var output = Path.GetFullPath("packaging.dot");
            Console.WriteLine("Output: {0}", output);

            using (var writer = new StreamWriter(output))
            {
                writer.WriteLine("digraph {");

                var clusters = new Dictionary<string, List<string>>();

                foreach (var cluster in myPackage.Clusters)
                {
                    clusters[cluster.Name] = new List<string>();
                }

                foreach (var node in myTypes.Select(GraphUtils.Node).Distinct())
                {
                    if (!edges.Any(e => e.Item1 == node || e.Item2 == node))
                    {
                        continue;
                    }

                    var nodeDesc = string.Format("  \"{0}\" [label = {1}]", node.FullName, node.Name);

                    // in case multiple cluster match we just take the first one
                    var matchedCluster = myPackage.Clusters.FirstOrDefault(c => c.Matches(node.FullName));
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

                writer.WriteLine();

                foreach (var edge in edges)
                {
                    writer.WriteLine("  \"{0}\" -> \"{1}\"", edge.Item1.FullName, edge.Item2.FullName);
                }

                writer.WriteLine("}");
            }
        }
    }
}
