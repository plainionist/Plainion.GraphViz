using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Windows.Markup;
using System.Xml;
using Plainion.GraphViz.Presentation;

namespace Plainion.GraphViz.Viewer.Configuration
{
    public class Config
    {
        private const string Filename = "Plainion.GraphViz.Viewer.xaml";

        private string myConfigFolder;
        private string myDotToolsHome;

        public Config()
        {
            LabelConversion = new List<ILabelConversionStep>();
            NodeIdAsDefaultToolTip = true;
        }

        public string DotToolsHome
        {
            get { return myDotToolsHome; }
            set { myDotToolsHome = value == null ? null : Path.GetFullPath(value); }
        }

        public bool NodeIdAsDefaultToolTip { get; set; }

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
        public List<ILabelConversionStep> LabelConversion
        {
            get;
            private set;
        }

        internal static Config Load(string dataRoot)
        {
            var configFile = Path.Combine(dataRoot, Filename);
            if (File.Exists(configFile))
            {
                return Deserialize(dataRoot, configFile);
            }

            {
                var config = LoadDefaults();
                config.myConfigFolder = dataRoot;
                return config;
            }
        }

        private static Config Deserialize(string configFolder, string configFile)
        {
            var config = (Config)XamlReader.Load(XmlReader.Create(configFile));
            config.myConfigFolder = configFolder;
            return config;
        }

        internal static Config LoadDefaults()
        {
            var binFolder = Path.GetDirectoryName(typeof(Config).Assembly.Location);
            var configFile = Path.Combine(binFolder, Filename);
            if (File.Exists(configFile))
            {
                return Deserialize(binFolder, configFile);
            }

            throw new InvalidOperationException("Could not find config file");
        }

        internal void Save()
        {
            XamlWriter.Save(this, XmlWriter.Create(Path.Combine(myConfigFolder, Filename)));
        }
    }
}
