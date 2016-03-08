using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Media;
using Plainion.GraphViz.Modules.CodeInspection.Packaging.Spec;
using Plainion.GraphViz.Presentation;

namespace Plainion.GraphViz.Modules.CodeInspection.Packaging.Services
{
    class AnalyzeCrossPackageDependencies : AnalyzeBase
    {
        private static Brush[] Colors = {Brushes.LightBlue, Brushes.LightGreen, Brushes.LightGray };

        private readonly Dictionary<string, List<Type>> myPackages = new Dictionary<string, List<Type>>();

        protected override void Load()
        {
            foreach (var package in Config.Packages)
            {
                CancellationToken.ThrowIfCancellationRequested();

                myPackages[package.Name] = Load(package)
                    .SelectMany(asm => asm.GetTypes())
                    .ToList();
            }
        }

        protected override Task<Tuple<Type, Type>[]>[] Analyze()
        {
            return Config.Packages
                .SelectMany(p => myPackages[p.Name]
                    .Select(t => new
                    {
                        Package = p,
                        Type = t
                    })
                )
                .Select(e => Task.Run<Tuple<Type, Type>[]>(() => Analyze(e.Package, e.Type), CancellationToken))
                .ToArray();
        }

        private Tuple<Type, Type>[] Analyze(Package package, Type type)
        {
            Console.Write(".");

            CancellationToken.ThrowIfCancellationRequested();

            return new Reflector(AssemblyLoader, type).GetUsedTypes()
                .Where(usedType => IsForeignPackage(package, usedType))
                .Select(usedType => GraphUtils.Edge(type, usedType))
                .ToArray();
        }

        private bool IsForeignPackage(Package package, Type dep)
        {
            return myPackages.Where(e => e.Key != package.Name).Any(entry => entry.Value.Contains(dep));
        }

        protected override AnalysisDocument GenerateDocument(IReadOnlyCollection<Tuple<Type, Type>> edges)
        {
            var doc = new AnalysisDocument();

            for (int i = 0; i < Config.Packages.Count; ++i)
            {
                var package = Config.Packages[i];

                foreach (var node in myPackages[package.Name].Select(GraphUtils.Node).Distinct())
                {
                    if (!edges.Any(e => e.Item1 == node || e.Item2 == node))
                    {
                        continue;
                    }

                    doc.AddNode(node.FullName);
                    doc.Add(new Caption(node.FullName, node.Name));
                    doc.Add(new NodeStyle(node.FullName) { FillColor =  Colors[i % Colors.Length] });

                    // in case multiple cluster match we just take the first one
                    var matchedCluster = package.Clusters.FirstOrDefault(c => c.Matches(node.FullName));
                    if (matchedCluster != null)
                    {
                        doc.AddToCluster(matchedCluster.Name, node.FullName);
                    }
                }
            }

            foreach (var edge in edges)
            {
                doc.AddEdge(edge.Item1.FullName, edge.Item2.FullName);
            }

            return doc;
        }
    }
}
