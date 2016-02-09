using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Plainion.GraphViz.Modules.Documents.DotLang;
using Plainion.GraphViz.Presentation;

namespace Plainion.GraphViz.Modules.Documents
{
    // http://www.graphviz.org/doc/info/lang.html
    // inspired by: https://github.com/devshorts/LanguageCreator
    public class DotLangPureDocument : AbstractGraphDocument, ICaptionDocument, IStyleDocument
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

        protected internal void Add(Caption caption)
        {
            myCaptions.Add(caption);
        }

        protected internal void Add(NodeStyle style)
        {
            myNodeStyles.Add(style);
        }

        protected internal void Add(EdgeStyle style)
        {
            myEdgeStyles.Add(style);
        }

        internal void Read(TextReader reader)
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
