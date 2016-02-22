
using System;
using System.Collections;
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

                writer.WriteLine();

                foreach (var edge in edges)
                {
                    writer.WriteLine("  \"{0}\" -> \"{1}\"", edge.Item1, edge.Item2);
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

        // TODO: generics
        // TODO: method body
        private IEnumerable<Tuple<string, string>> Analyze(Package package, Type type)
        {
            Console.WriteLine("  {0}", type.FullName);

            // base classes
            {
                var baseType = type.BaseType;
                while (baseType != null && baseType != typeof(object))
                {
                    if (IsForeignPackage(package, baseType))
                    {
                        yield return Edge(type, baseType);
                    }

                    baseType = baseType.BaseType;
                }
            }

            // interfaces
            {
                foreach (var iface in type.GetInterfaces())
                {
                    if (IsForeignPackage(package, iface))
                    {
                        yield return Edge(type, iface);
                    }
                }
            }

            // members
            {
                foreach (var field in type.GetFields(BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Static))
                {
                    if (IsForeignPackage(package, field.FieldType))
                    {
                        yield return Edge(type, field.FieldType);
                    }
                }

                foreach (var property in type.GetProperties(BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Static))
                {
                    if (IsForeignPackage(package, property.PropertyType))
                    {
                        yield return Edge(type, property.PropertyType);
                    }
                }

                foreach (var method in type.GetMethods(BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Static))
                {
                    if (IsForeignPackage(package, method.ReturnType))
                    {
                        yield return Edge(type, method.ReturnType);
                    }

                    foreach (var parameter in method.GetParameters())
                    {
                        if (parameter.ParameterType.HasElementType)
                        {
                            if (IsForeignPackage(package, parameter.ParameterType.GetElementType()))
                            {
                                yield return Edge(type, parameter.ParameterType.GetElementType());
                            }
                        }
                        else
                        {
                            if (IsForeignPackage(package, parameter.ParameterType))
                            {
                                yield return Edge(type, parameter.ParameterType);
                            }
                        }
                    }
                }

                foreach (var method in type.GetConstructors(BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Static))
                {
                    foreach (var parameter in method.GetParameters())
                    {
                        if (parameter.ParameterType.HasElementType)
                        {
                            if (IsForeignPackage(package, parameter.ParameterType.GetElementType()))
                            {
                                yield return Edge(type, parameter.ParameterType.GetElementType());
                            }
                        }
                        else
                        {
                            if (IsForeignPackage(package, parameter.ParameterType))
                            {
                                yield return Edge(type, parameter.ParameterType);
                            }
                        }
                    }
                }
            }

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

        private Tuple<string, string> Edge(Type type, Type baseType)
        {
            return new Tuple<string, string>(type.Name, baseType.Name);
        }
    }
}
