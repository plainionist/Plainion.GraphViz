
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Plainion.GraphViz.Modules.Documents;
using Plainion.GraphViz.Presentation;


namespace Plainion.GraphViz.Pioneer.Activities
{
    class AnalysisDocument : AbstractGraphDocument, ICaptionDocument, IStyleDocument
    {
        private List<Caption> myCaptions;
        private List<NodeStyle> myNodeStyles;
        private List<EdgeStyle> myEdgeStyles;

        public AnalysisDocument()
        {
            myCaptions = new List<Caption>();
            myNodeStyles = new List<NodeStyle>();
            myEdgeStyles = new List<EdgeStyle>();
        }

        public IEnumerable<Caption> Captions
        {
            get { return myCaptions; }
        }

        public IEnumerable<NodeStyle> NodeStyles
        {
            get { return myNodeStyles; }
        }

        public IEnumerable<EdgeStyle> EdgeStyles
        {
            get { return myEdgeStyles; }
        }

        internal void Add(Caption caption)
        {
            myCaptions.Add(caption);
        }

        internal void Add(NodeStyle style)
        {
            myNodeStyles.Add(style);
        }

        internal void Add(EdgeStyle style)
        {
            myEdgeStyles.Add(style);
        }

        protected override void Load()
        {
            throw new System.NotImplementedException();
        }
    }
}
