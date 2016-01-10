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
    public class DotLangPureDocument : AbstractGraphDocument, ICaptionDocument
    {
        private List<Caption> myCaptions;

        public IEnumerable<Caption> Captions
        {
            get { return myCaptions; }
        }

        protected override void Load()
        {
            using( var reader = new StreamReader( Filename ) )
            {
                Read( reader );
            }
        }

        protected internal void Add( Caption caption )
        {
            myCaptions.Add( caption );
        }

        internal void Read( TextReader reader )
        {
            myCaptions = new List<Caption>();
            
            var lexer = new Lexer( reader.ReadToEnd() );
            var parser = new Parser( lexer, this );
            parser.Parse();
        }
    }
}
