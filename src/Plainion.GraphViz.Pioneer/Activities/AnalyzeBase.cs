using System;
using System.Collections.Generic;
using System.Reflection;
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

            Execute();
        }

        protected abstract void Execute();

        protected IEnumerable<Assembly> Load(Package package)
        {
            Console.WriteLine("Loading package {0}", package.Name);

            return AssemblyLoader.Load(Config.AssemblyRoot, package);
        }
    }
}
