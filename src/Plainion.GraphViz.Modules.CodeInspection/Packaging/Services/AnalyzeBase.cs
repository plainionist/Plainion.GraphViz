using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Plainion.GraphViz.Model;
using Plainion.GraphViz.Modules.CodeInspection.Packaging;
using Plainion.GraphViz.Modules.CodeInspection.Packaging.Spec;
using Plainion.GraphViz.Presentation;

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

            return GenerateDocument(edges);
        }

        protected abstract void Load();

        protected abstract Task<Tuple<Type, Type>[]>[] Analyze();

        protected abstract AnalysisDocument GenerateDocument(IReadOnlyCollection<Tuple<Type, Type>> edges);

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
