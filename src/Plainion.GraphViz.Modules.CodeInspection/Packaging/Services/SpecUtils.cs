using System;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Windows.Markup;
using System.Xml;
using Plainion.GraphViz.Modules.CodeInspection.Packaging.Spec;

namespace Plainion.GraphViz.Modules.CodeInspection.Packaging.Services
{
    class SpecUtils
    {
        public static SystemPackaging Deserialize(string text)
        {
            var doc =  DeserializeCore( text );

            foreach( var cluster in doc.Packages.SelectMany( p => p.Clusters ) )
            {
                if( cluster.Id == null )
                {
                    cluster.Id = Guid.NewGuid().ToString();
                }
            }

            return doc;
        }

        private static SystemPackaging DeserializeCore( string text )
        {
            using( var reader = new StringReader( text ) )
            {
                using( var xmlReader = XmlReader.Create( reader ) )
                {
                    return ( SystemPackaging )XamlReader.Load( xmlReader );
                }
            }
        }

        public static string Serialize(SystemPackaging spec)
        {
            using (var writer = new StringWriter())
            {
                var settings = new XmlWriterSettings();
                settings.Indent = true;
                settings.IndentChars = "    ";
                settings.NewLineChars = Environment.NewLine;
                settings.NewLineHandling = NewLineHandling.Replace;
                settings.Encoding = Encoding.UTF8;

                using (var xmlWriter = XmlWriter.Create(writer, settings))
                {
                    XamlWriter.Save(spec, xmlWriter);
                    return writer.ToString();
                }
            }
        }

        public static string Zip(string str)
        {
            using (var msi = new MemoryStream(Encoding.UTF8.GetBytes(str)))
            {
                using (var mso = new MemoryStream())
                {
                    using (var gs = new GZipStream(mso, CompressionMode.Compress))
                    {
                        CopyTo(msi, gs);
                    }

                    return Convert.ToBase64String(mso.ToArray());
                }
            }
        }

        private static void CopyTo(Stream src, Stream dest)
        {
            var bytes = new byte[4096];

            int cnt;

            while ((cnt = src.Read(bytes, 0, bytes.Length)) != 0)
            {
                dest.Write(bytes, 0, cnt);
            }
        }

        public static string Unzip(string compressedString)
        {
            using (var msi = new MemoryStream(Convert.FromBase64String(compressedString)))
            {
                using (var mso = new MemoryStream())
                {
                    using (var gs = new GZipStream(msi, CompressionMode.Decompress))
                    {
                        CopyTo(gs, mso);
                    }

                    return Encoding.UTF8.GetString(mso.ToArray());
                }
            }
        }
    }
}
