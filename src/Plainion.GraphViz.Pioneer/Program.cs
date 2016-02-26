using System;
using System.IO;
using System.Windows.Markup;
using System.Xml;
using Plainion.GraphViz.Pioneer.Activities;
using Plainion.GraphViz.Pioneer.Spec;

namespace Plainion.GraphViz.Pioneer
{
    class Program
    {
        private static string myPackageName;
        private static string myConfigFile;
        
        private static void Main(string[] args)
        {
            for (int i = 0; i < args.Length; ++i)
            {
                if (args[i] == "-p")
                {
                    myPackageName = args[1];
                }
                else
                {
                    myConfigFile = args[i];
                }
            }

            if (myConfigFile == null)
            {
                Console.WriteLine("Config file missing");
                Environment.Exit(1);
            }

            if (!File.Exists(myConfigFile))
            {
                Console.WriteLine("Config file does not exist: " + myConfigFile);
                Environment.Exit(1);
            }

            var config = (Config)XamlReader.Load(XmlReader.Create(myConfigFile));

            var analyzer = CreateAnalyzer(config);
            analyzer.Execute(config);
        }

        private static AnalyzeBase CreateAnalyzer(Config config)
        {
            if (myPackageName == null)
            {
                return new AnalyzePackageDependencies();
            }
            else
            {
                return new AnalyzeSubSystemDependencies()
                {
                    PackageName = myPackageName
                };
            }
        }
    }
}
