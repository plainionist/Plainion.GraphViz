using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

using Plainion.GraphViz.Pioneer.Services;
using Plainion.GraphViz.Pioneer.Spec;

namespace Plainion.GraphViz.Pioneer.Activities
{
    abstract class AnalyzeBase
    {
        protected AnalyzeBase()
        {
            AssemblyLoader = new AssemblyLoader();
        }

        protected Config Config { get; private set; }

        protected AssemblyLoader AssemblyLoader { get; private set; }

        public void Execute(Config config)
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

            DrawGraph(edges);
        }

        protected abstract void Load();

        protected abstract Task<Tuple<Type, Type>[]>[] Analyze();

        protected abstract void DrawGraph(IReadOnlyCollection<Tuple<Type, Type>> edges);

        protected IEnumerable<Assembly> Load(Package package)
        {
            Console.WriteLine("Loading package {0}", package.Name);

            return AssemblyLoader.Load(Config.AssemblyRoot, package);
        }
    }
}
