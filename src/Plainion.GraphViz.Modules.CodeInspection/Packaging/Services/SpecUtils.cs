using System.IO;
using System.Windows.Markup;
using System.Xml;
using Plainion.GraphViz.Modules.CodeInspection.Packaging.Spec;

namespace Plainion.GraphViz.Modules.CodeInspection.Packaging.Services
{
    class SpecUtils
    {
        public static SystemPackaging Deserialize(string text)
        {
            using( var reader = new StringReader( text ) )
            {
                return ( SystemPackaging )XamlReader.Load( XmlReader.Create( reader ) );
            }
        }
    }
}
