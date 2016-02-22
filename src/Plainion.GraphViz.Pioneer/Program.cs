using System;
using System.IO;
using System.Windows.Markup;
using System.Xml;

using Plainion.GraphViz.Pioneer.Packaging;

namespace Plainion.GraphViz.Pioneer
{
    class Program
    {
        static void Main(string[] args)
        {
            if (args.Length == 0)
            {
                Console.WriteLine("Config file missing");
                Environment.Exit(1);
            }

            var configFile = args[0];

            if (!File.Exists(configFile))
            {
                Console.WriteLine("Config file does not exist: " + configFile);
                Environment.Exit(1);
            }

            var config = XamlReader.Load(XmlReader.Create(configFile));

            var analyzer = new Analyzer();
            analyzer.Execute(config);
        }
    }
}
