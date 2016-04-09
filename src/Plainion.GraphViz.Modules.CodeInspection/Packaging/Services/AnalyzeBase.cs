using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;
using Plainion.GraphViz.Modules.CodeInspection.Packaging.Spec;

namespace Plainion.GraphViz.Modules.CodeInspection.Packaging.Services
{
    abstract class AnalyzeBase
    {
        protected AnalyzeBase()
        {
            AssemblyLoader = new AssemblyLoader();
        }

        protected SystemPackaging Config { get; private set; }

        protected CancellationToken CancellationToken { get; private set; }

        protected AssemblyLoader AssemblyLoader { get; private set; }

        public AnalysisDocument Execute(SystemPackaging config, CancellationToken cancellationToken)
        {
            Config = config;
            CancellationToken = cancellationToken;

            Load();

            Console.WriteLine("Analyzing ...");

            var edges = Analyze()
                .Distinct()
                .ToList();

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

            Console.WriteLine("Building Graph ...");
            Debugger.Launch();
            return GenerateDocument(edges);
        }

        protected abstract void Load();

        protected abstract Edge[] Analyze();

        protected abstract AnalysisDocument GenerateDocument( IReadOnlyCollection<Edge> edges );

        protected IEnumerable<Assembly> Load(Package package)
        {
            Console.WriteLine("Assembly root {0}", Path.GetFullPath(Config.AssemblyRoot));
            Console.WriteLine("Loading package {0}", package.Name);

            return package.Includes
                .SelectMany(i => Directory.GetFiles(Config.AssemblyRoot, i.Pattern))
                .Where(file => !package.Excludes.Any(e => e.Matches(file)))
                .Select(AssemblyLoader.Load)
                .Where(asm => asm != null)
                .ToList();
        }
    }
}
