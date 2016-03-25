using System;
using System.IO;
using System.Windows.Markup;
using System.Xml;
using Plainion.GraphViz.Modules.CodeInspection.Packaging.Spec;

namespace Plainion.GraphViz.Modules.CodeInspection.Packaging.Services
{
    class SpecUtils
    {
        public static SystemPackaging Deserialize( string text )
        {
            using( var reader = new StringReader( text ) )
            {
                using( var xmlReader = XmlReader.Create( reader ) )
                {
                    return ( SystemPackaging )XamlReader.Load( xmlReader );
                }
            }
        }

        public static string Serialize( SystemPackaging spec )
        {
            using( var writer = new StringWriter() )
            {
                var settings = new XmlWriterSettings();
                settings.Indent = true;
                settings.IndentChars = "    ";
                settings.NewLineChars = Environment.NewLine;
                settings.NewLineHandling = NewLineHandling.Replace;

                using( var xmlWriter = XmlWriter.Create( writer, settings ) )
                {
                    XamlWriter.Save( spec, xmlWriter );
                    return writer.ToString();
                }
            }
        }
    }
}
