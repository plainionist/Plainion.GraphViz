
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace Plainion.GraphViz.Pioneer.Packaging
{
    class Analyzer
    {
        private static string[] Colors = new[] { "lightblue", "lightgreen", "lightgray" };

        private Dictionary<string, List<Type>> myPackages = new Dictionary<string, List<Type>>();

        // http://stackoverflow.com/questions/24680054/how-to-get-the-list-of-methods-called-from-a-method-using-reflection-in-c-sharp
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
                .Select(p => Task.Run<Tuple<string, string>[]>(() => Analyze(p)))
                .ToArray();

            Task.WaitAll(tasks);

            var edges = tasks
                .SelectMany(t => t.Result)
                .ToList();

            var output = Path.GetFullPath("packaging.dot");
            Console.WriteLine("Output: {0}", output);

            using (var writer = new StreamWriter(output))
            {
                writer.WriteLine("digraph {");

                for (int i = 0; i < config.Packages.Count; ++i)
                {
                    foreach (var type in myPackages[config.Packages[i].Name])
                    {
                        var node = type.Name;
                        if (!edges.Any(e => e.Item1 == node || e.Item2 == node))
                        {
                            continue;
                        }

                        writer.WriteLine("  \"{0}\" [color = {1}]", node, Colors[i]);
                    }
                }

                foreach (var edge in edges)
                {
                    writer.WriteLine("\"{0}\" -> \"{1}\"", edge.Item1, edge.Item2);
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

        private Tuple<string, string>[] Analyze(Package p)
        {
            return myPackages[p.Name]
                .SelectMany(t => Analyze(p, t))
                .ToArray();
        }

        private IEnumerable<Tuple<string, string>> Analyze(Package package, Type type)
        {
            Console.WriteLine("  {0}", type.FullName);

            var baseType = type.BaseType;
            while (baseType != null && baseType != typeof(object))
            {
                foreach (var entry in myPackages)
                {
                    if (entry.Key == package.Name)
                    {
                        continue;
                    }

                    if (entry.Value.Contains(baseType))
                    {
                        yield return Edge(type, baseType);
                    }
                }

                baseType = baseType.BaseType;
            }
        }

        private Tuple<string, string> Edge(Type type, Type baseType)
        {
            return new Tuple<string, string>(type.Name, baseType.Name);
        }
    }
}
