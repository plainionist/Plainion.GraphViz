using System;
using System.IO;
using System.Linq;
using NUnit.Framework;

namespace Plainion.GraphViz.Modules.Documents.Tests
{
    [TestFixture]
    public class DotLangPureDocumentTests
    {
        [Test]
        public void Read_SimpleGraphWithTwoNodesAndNewLines_NotesDetected()
        {
            var document = new DotLangPureDocument();

            using( var reader = new StringReader( @"graph { 
    n1
    n2
}" ) )
            {
                document.Read( reader );
            }

            Assert.That( document.Graph.Nodes.Select( n => n.Id ), Is.EquivalentTo( new[] { "n1", "n2" } ) );
        }

        [Test]
        public void Read_SimpleDirectedGraphWithTwoNodesAndSemiColon_NotesDetected()
        {
            var document = new DotLangPureDocument();

            using( var reader = new StringReader( @"digraph { n1; n2; }" ) )
            {
                document.Read( reader );
            }

            Assert.That( document.Graph.Nodes.Select( n => n.Id ), Is.EquivalentTo( new[] { "n1", "n2" } ) );
        }
    }
}
