using System.Collections.Generic;
using System.IO;
using Plainion.GraphViz.Modules.Documents.DotLang;
using Plainion.GraphViz.Presentation;

namespace Plainion.GraphViz.Modules.Documents
{
    // http://www.graphviz.org/doc/info/lang.html
    // inspired by: https://github.com/devshorts/LanguageCreator
    class DotLangDocument : AbstractGraphDocument, ICaptionDocument, IStyleDocument
    {
        private List<Caption> myCaptions;
        private List<NodeStyle> myNodeStyles;
        private List<EdgeStyle> myEdgeStyles;

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

        protected override void Load()
        {
            using (var reader = new StreamReader(Filename))
            {
                Read(reader);
            }
        }

        public void Add(Caption caption)
        {
            myCaptions.Add(caption);
        }

        public void Add(NodeStyle style)
        {
            myNodeStyles.Add(style);
        }

        public void Add(EdgeStyle style)
        {
            myEdgeStyles.Add(style);
        }

        public void Read(TextReader reader)
        {
            myCaptions = new List<Caption>();
            myNodeStyles = new List<NodeStyle>();
            myEdgeStyles = new List<EdgeStyle>();

            var lexer = new Lexer(reader.ReadToEnd());
            var parser = new Parser(lexer, this);
            parser.Parse();
        }
    }
}
