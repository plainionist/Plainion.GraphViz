using System.Collections.Generic;
using System.Linq;
using System.Windows.Markup;
using System.Xml.Linq;
using Plainion.GraphViz.Presentation;

namespace Plainion.GraphViz.Modules.Documents
{
    class ToolTipsDocument : IDocument
    {
        private List<ToolTipContent> myToolTips;

        public ToolTipsDocument()
        {
            myToolTips = new List<ToolTipContent>();
        }

        public string Filename
        {
            get;
            private set;
        }

        public IEnumerable<ToolTipContent> ToolTips { get { return myToolTips; } }

        public void Load( string path )
        {
            Filename = path;

            myToolTips = new List<ToolTipContent>();

            var root = XElement.Load( Filename );

            foreach( var tip in root.Elements( "Tip" ) )
            {
                string ownerId = tip.Attribute( "ItemId" ).Value;
                var content = XamlReader.Load( tip.Elements().Single().CreateReader() );

                myToolTips.Add( new ToolTipContent( ownerId, content ) );
            }
        }
    }
}
