using System.Collections.Generic;
using System.IO;
using Plainion.GraphViz.Dot;
using Plainion.GraphViz.Model;
using Plainion.GraphViz.Presentation;

namespace Plainion.GraphViz.Modules.Documents
{
    // http://www.graphviz.org/doc/info/lang.html
    // keep it as wrapper around plaindoc for now instead of inheritance as later on it should be independent anyway
    public class DotLangDocument : AbstractGraphDocument, IStyleDocument, ICaptionDocument, ILayoutDocument
    {
        private DotPlainDocument myPlainDocument;
        private DotToDotPlainConverter myConverter;

        public DotLangDocument( DotToDotPlainConverter converter )
        {
            myConverter = converter;

            myPlainDocument = new DotPlainDocument();
        }

        public IEnumerable<Caption> Captions
        {
            get { return myPlainDocument.Captions; }
        }

        public IEnumerable<NodeLayout> NodeLayouts
        {
            get { return myPlainDocument.NodeLayouts; }
        }

        public IEnumerable<EdgeLayout> EdgeLayouts
        {
            get { return myPlainDocument.EdgeLayouts; }
        }

        public IEnumerable<NodeStyle> NodeStyles
        {
            get { return myPlainDocument.NodeStyles; }
        }

        public IEnumerable<EdgeStyle> EdgeStyles
        {
            get { return myPlainDocument.EdgeStyles; }
        }

        public override IGraph Graph
        {
            get { return myPlainDocument.Graph; }
        }

        protected override void Load()
        {
            var dotFile = new FileInfo( Filename );
            var plainFile = new FileInfo( Path.ChangeExtension( Filename, ".plain" ) );

            if( dotFile.LastWriteTime > plainFile.LastWriteTime )
            {
                myConverter.Convert( dotFile, plainFile );
            }

            myPlainDocument = new DotPlainDocument();
            myPlainDocument.Load( plainFile.FullName );
        }
    }
}
