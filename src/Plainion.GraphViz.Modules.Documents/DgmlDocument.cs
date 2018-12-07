using System.Collections.Generic;
using System.Xml.Linq;
using Plainion.GraphViz.Presentation;

namespace Plainion.GraphViz.Modules.Documents
{
    public class DgmlDocument : AbstractGraphDocument, ICaptionDocument
    {
        private List<Caption> myCaptions;

        public DgmlDocument()
        {
            myCaptions = new List<Caption>();
        }

        public IEnumerable<Caption> Captions { get { return myCaptions; } }

        protected override void Load()
        {
            myCaptions = new List<Caption>();

            var root = XElement.Load(Filename);

            if (root.Element(XN("Nodes")) != null)
            {
                foreach (var xmlNode in root.Element(XN("Nodes")).Elements(XN("Node")))
                {
                    var node = TryAddNode(xmlNode.Attribute("Id").Value);
                    if (node != null)
                    {
                        var xmlLabel = xmlNode.Attribute("Label");
                        if (xmlLabel != null)
                        {
                            myCaptions.Add(new Caption(node.Id, xmlLabel.Value));
                        }
                    }
                }
            }

            foreach (var xmlLink in root.Element(XN("Links")).Elements(XN("Link")))
            {
                TryAddEdge(xmlLink.Attribute("Source").Value, xmlLink.Attribute("Target").Value);
            }
        }

        private static XName XN(string localName)
        {
            return XName.Get(localName, "http://schemas.microsoft.com/vs/2009/dgml");
        }
    }
}
