﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Windows.Markup;
using System.Xml;
using Plainion.GraphViz.Presentation;

namespace Plainion.GraphViz.Viewer.Configuration
{
    // needs to be public in order to be serializable/deserializable with Xaml serializer
    public class Config
    {
        private const string Filename = "Plainion.GraphViz.Viewer.xaml";

        private string myConfigFolder;

        public Config()
        {
            LabelConversion = new List<ILabelConversionStep>();
        }

        [DesignerSerializationVisibility( DesignerSerializationVisibility.Content )]
        public List<ILabelConversionStep> LabelConversion { get; private set; }

        public static Config Load( string dataRoot )
        {
            var configFile = Path.Combine( dataRoot, Filename );
            if( File.Exists( configFile ) )
            {
                return Deserialize( dataRoot, configFile );
            }

            {
                var config = LoadDefaults();
                config.myConfigFolder = dataRoot;
                return config;
            }
        }

        private static Config Deserialize( string configFolder, string configFile )
        {
            var config = ( Config )XamlReader.Load( XmlReader.Create( configFile ) );
            config.myConfigFolder = configFolder;
            return config;
        }

        public static Config LoadDefaults()
        {
            var binFolder = Path.GetDirectoryName( typeof( Config ).Assembly.Location );
            var configFile = Path.Combine( binFolder, Filename );
            if( File.Exists( configFile ) )
            {
                return Deserialize( binFolder, configFile );
            }

            throw new InvalidOperationException( "Could not find config file" );
        }

        public void Save()
        {
            XamlWriter.Save( this, XmlWriter.Create( Path.Combine( myConfigFolder, Filename ) ) );
        }
    }
}
