using System;
using System.Collections.Generic;
using System.Linq;
using Plainion.GraphViz.Modules.CodeInspection.Packaging.Spec;

namespace Plainion.GraphViz.Modules.CodeInspection.Packaging.Services
{
    class AnalyzeCrossPackageDependencies : AnalyzeBase
    {
        private static string[] Colors = { "LightBlue", "LightGreen", "LightGray" };

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

        protected override Edge[] Analyze()
        {
            return Config.Packages
                .SelectMany(p => myPackages[p.Name]
                    .Select(t => new
                    {
                        Package = p,
                        Type = t
                    })
                )
                .AsParallel()
                .WithCancellation(CancellationToken)
                .SelectMany(e => Analyze(e.Package, e.Type))
                .ToArray();
        }

        private IEnumerable<Edge> Analyze(Package package, Type type)
        {
            Console.Write(".");

            CancellationToken.ThrowIfCancellationRequested();

            return new Reflector(AssemblyLoader, type).GetUsedTypes()
                .Where(edge => IsForeignPackage(package, edge.Target))
                .Select(edge => GraphUtils.Edge(edge))
                .Where(edge => edge.Source != edge.Target);
        }

        private bool IsForeignPackage(Package package, Type dep)
        {
            return myPackages.Where(e => e.Key != package.Name).Any(entry => entry.Value.Contains(dep));
        }

        protected override AnalysisDocument GenerateDocument(IReadOnlyCollection<Edge> edges)
        {
            var doc = new AnalysisDocument();

            var nodesWithEdgesIndex = new HashSet<Type>();
            foreach (var edge in edges)
            {
                nodesWithEdgesIndex.Add(edge.Source);
                nodesWithEdgesIndex.Add(edge.Target);
            }
            
            for (int i = 0; i < Config.Packages.Count; ++i)
            {
                var package = Config.Packages[i];

                foreach (var node in myPackages[package.Name].Select(GraphUtils.Node).Distinct())
                {
                    if (!nodesWithEdgesIndex.Contains(node))
                    {
                        continue;
                    }

                    doc.Add(node, package, Colors[i % Colors.Length]);
                }
            }

            doc.Add(edges);

            return doc;
        }
    }
}
