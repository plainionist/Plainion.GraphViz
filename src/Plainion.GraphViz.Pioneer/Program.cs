using System;
using System.Diagnostics;
using System.IO;
using System.Windows.Markup;
using System.Xml;
using Plainion.GraphViz.Modules.Reflection.Analysis.Packaging.Spec;
using Plainion.GraphViz.Pioneer.Activities;

namespace Plainion.GraphViz.Pioneer
{
    class Program
    {
        private static string myPackageName;
        private static string myConfigFile;
        private static string myOutputFile;

        private static void Main(string[] args)
        {
            myOutputFile = Path.GetFullPath(".");

            for (int i = 0; i < args.Length; ++i)
            {
                if (args[i] == "-p")
                {
                    myPackageName = args[i + 1];
                }
                else if (args[i] == "-o")
                {
                    myOutputFile = args[i + 1];
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

            var config = (SystemPackaging)XamlReader.Load(XmlReader.Create(myConfigFile));

            var analyzer = CreateAnalyzer(config);
            analyzer.OutputFile = myOutputFile;
            analyzer.Execute(config);
        }

        private static AnalyzeBase CreateAnalyzer(SystemPackaging config)
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
