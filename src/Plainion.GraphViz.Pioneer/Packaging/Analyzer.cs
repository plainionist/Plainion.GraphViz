
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;

namespace Plainion.GraphViz.Pioneer.Packaging
{
    class Analyzer
    {
        private static string[] Colors = new[] { "lightblue", "lightgreen", "lightgray" };

        private Dictionary<string, List<Type>> myPackages = new Dictionary<string, List<Type>>();

        internal void Execute(object rawConfig)
        {
            var config = (Config)rawConfig;

            foreach (var package in config.Packages)
            {
                Console.WriteLine("Loading package {0}", package.Name);

                var assemblies = package.Includes
                    .SelectMany(i => Directory.GetFiles(config.AssemblyRoot, i.Pattern))
                    .Where(file => !package.Excludes.Any(e => e.Matches(file)))
                    .Select(Load)
                    .Where(asm => asm != null)
                    .ToList();

                myPackages[package.Name] = assemblies
                    .SelectMany(asm => asm.GetTypes())
                    .ToList();
            }

            Console.WriteLine("Analyzing ...");

            var tasks = config.Packages
                .Select(p => Task.Run<Tuple<Type, Type>[]>(() => Analyze(p)))
                .ToArray();

            Task.WaitAll(tasks);

            var edges = tasks
                .SelectMany(t => t.Result)
                .Distinct()
                .ToList();

            var output = Path.GetFullPath("packaging.dot");
            Console.WriteLine("Output: {0}", output);

            using (var writer = new StreamWriter(output))
            {
                writer.WriteLine("digraph {");

                for (int i = 0; i < config.Packages.Count; ++i)
                {
                    var package = config.Packages[i];

                    var clusters = new Dictionary<string, List<string>>();

                    foreach (var cluster in package.Clusters)
                    {
                        clusters[cluster.Name] = new List<string>();
                    }

                    foreach (var node in myPackages[package.Name].Select(Node).Distinct())
                    {
                        if (!edges.Any(e => e.Item1 == node || e.Item2 == node))
                        {
                            continue;
                        }

                        var nodeDesc = string.Format("  \"{0}\" [color = {1}]", node.Name, Colors[i]);

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
                    writer.WriteLine("  \"{0}\" -> \"{1}\"", edge.Item1.Name, edge.Item2.Name);
                }

                writer.WriteLine("}");
            }
        }

        private Assembly Load(string path)
        {
            try
            {
                Console.WriteLine("Loading {0}", path);

                return Assembly.LoadFrom(path);
            }
            catch
            {
                Console.WriteLine("ERROR: failed to load assembly {0}", path);
                return null;
            }
        }

        private Tuple<Type, Type>[] Analyze(Package p)
        {
            return myPackages[p.Name]
                .SelectMany(t => Analyze(p, t))
                .ToArray();
        }

        private IEnumerable<Tuple<Type, Type>> Analyze(Package package, Type type)
        {
            Console.Write(".");

            return new Reflector(type).GetUsedTypes()
                .Where(usedType => IsForeignPackage(package, usedType))
                .Select(usedType => Edge(type, usedType));
        }

        private bool IsForeignPackage(Package package, Type dep)
        {
            foreach (var entry in myPackages.Where(e => e.Key != package.Name))
            {
                if (entry.Value.Contains(dep))
                {
                    return true;
                }
            }

            return false;
        }

        private Tuple<Type, Type> Edge(Type source, Type target)
        {
            return new Tuple<Type, Type>(Node(source), Node(target));
        }

        private Type Node(Type type)
        {
            var nodeType = type;

            if (type.GetCustomAttribute(typeof(CompilerGeneratedAttribute), true) != null)
            {
                nodeType = type.DeclaringType;
            }

            if (nodeType == null)
            {
                // e.g. code generated from Xml like ResourceManager
                nodeType = type;
            }

            return nodeType;
        }
    }
}
