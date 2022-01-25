using System.Collections.Generic;
using System.IO;
using Plainion.GraphViz.Dot;
using Plainion.GraphViz.Presentation;

namespace Plainion.GraphViz.Modules.Documents
{
    // http://www.graphviz.org/doc/info/lang.html
    class DotPlainDocument : AbstractGraphDocument, IStyleDocument, ICaptionDocument, ILayoutDocument
    {
        private List<NodeLayout> myNodeLayouts;
        private List<EdgeLayout> myEdgeLayouts;
        private List<NodeStyle> myNodeStyles;
        private List<EdgeStyle> myEdgeStyles;
        private List<Caption> myCaptions;

        public DotPlainDocument()
        {
            Initialize();
        }

        private void Initialize()
        {
            myCaptions = new List<Caption>();

            myNodeLayouts = new List<NodeLayout>();
            myEdgeLayouts = new List<EdgeLayout>();

            myNodeStyles = new List<NodeStyle>();
            myEdgeStyles = new List<EdgeStyle>();
        }

        protected override void Load()
        {
            Initialize();

            using (var reader = new DotPlainReader(new StreamReader(Filename)))
            {
                var parser = new DotPlainParser(reader);

                parser.Open();

                while (parser.MoveNextEntry("node"))
                {
                    var node = TryAddNode(parser.ReadId());
                    if (node != null)
                    {
                        var layout = parser.ReadNodeLayout(node.Id);
                        myNodeLayouts.Add(layout);

                        var caption = parser.ReadLabel(node.Id);
                        myCaptions.Add(caption);

                        var style = parser.ReadNodeStyle(node.Id);
                        myNodeStyles.Add(style);
                    }
                }

                while (parser.MoveNextEntry("edge"))
                {
                    var sourceNodeId = parser.ReadId();
                    var targetNodeId = parser.ReadId();

                    var edge = TryAddEdge(sourceNodeId, targetNodeId);
                    if (edge != null)
                    {
                        var layout = parser.ReadEdgeLayout(edge.Id);
                        myEdgeLayouts.Add(layout);

                        // TODO: we should check for labels

                        var style = parser.ReadEdgeStyle(edge.Id);
                        myEdgeStyles.Add(style);
                    }
                }
            }
        }

        public IEnumerable<Caption> Captions
        {
            get { return myCaptions; }
        }

        public IEnumerable<NodeLayout> NodeLayouts
        {
            get { return myNodeLayouts; }
        }

        public IEnumerable<EdgeLayout> EdgeLayouts
        {
            get { return myEdgeLayouts; }
        }

        public IEnumerable<NodeStyle> NodeStyles
        {
            get { return myNodeStyles; }
        }

        public IEnumerable<EdgeStyle> EdgeStyles
        {
            get { return myEdgeStyles; }
        }
    }
}
