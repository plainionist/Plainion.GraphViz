using System;
using System.IO;
using System.Windows.Markup;
using System.Xml;
using Plainion.GraphViz.Pioneer.Activities;
using Plainion.GraphViz.Pioneer.Services;
using Plainion.GraphViz.Pioneer.Spec;
using Plainion.GraphViz.Pioneer.Tests;


namespace Plainion.GraphViz.Pioneer
{
    class Program
    {
        private static void Main(string[] args)
        {
            //var reflector = new Reflector(new AssemblyLoader(), typeof(ReflectorTests));
            //var x = reflector.GetUsedTypes();

            string packageName = null;
            string configFile = null;

            for (int i = 0; i < args.Length; ++i)
            {
                if (args[i] == "-p")
                {
                    packageName = args[1];
                }
                else
                {
                    configFile = args[i];
                }
            }

            if (configFile == null)
            {
                Console.WriteLine("Config file missing");
                Environment.Exit(1);
            }

            if (!File.Exists(configFile))
            {
                Console.WriteLine("Config file does not exist: " + configFile);
                Environment.Exit(1);
            }

            var config = (Config)XamlReader.Load(XmlReader.Create(configFile));

            if (packageName == null)
            {
                var analyzer = new AnalyzePackageDependencies();
                analyzer.Execute(config);
            }
            else
            {
                var analyzer = new AnalyzeSubSystemDependencies();
                analyzer.Execute(config, packageName);
            }
        }
    }
}
