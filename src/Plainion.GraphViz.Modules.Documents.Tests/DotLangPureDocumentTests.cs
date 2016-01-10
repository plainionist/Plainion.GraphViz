using System;
using System.IO;
using System.Linq;
using NUnit.Framework;
using Plainion.GraphViz.Model;

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

        [Test]
        public void Read_NodeIdWithQotes_NotesDetected()
        {
            var document = new DotLangPureDocument();

            using( var reader = new StringReader( "digraph { \"Node 1\"; \"Node 2\"; }" ) )
            {
                document.Read( reader );
            }

            Assert.That( document.Graph.Nodes.Select( n => n.Id ), Is.EquivalentTo( new[] { "Node 1", "Node 2" } ) );
        }

        [Test]
        public void Read_CPlusPlusStyleComment_Ignored()
        {
            var document = new DotLangPureDocument();

            using( var reader = new StringReader( @"graph { 
    n1
   /* ignore this */
    n2
}" ) )
            {
                document.Read( reader );
            }

            Assert.That( document.Graph.Nodes.Select( n => n.Id ), Is.EquivalentTo( new[] { "n1", "n2" } ) );
        }

        [Test]
        public void Read_CSharpStyleComment_Ignored()
        {
            var document = new DotLangPureDocument();

            using( var reader = new StringReader( @"graph { 
    n1
    // ignore this
    n2
}" ) )
            {
                document.Read( reader );
            }

            Assert.That( document.Graph.Nodes.Select( n => n.Id ), Is.EquivalentTo( new[] { "n1", "n2" } ) );
        }

        [Test]
        public void Read_SimpleGraphWithTwoEdgesAndNewLines_NotesDetected()
        {
            var document = new DotLangPureDocument();

            using( var reader = new StringReader( @"graph { 
    n1 -> n2
    n2 -> n3
}" ) )
            {
                document.Read( reader );
            }

            Assert.That( document.Graph.Nodes.Select( n => n.Id ), Is.EquivalentTo( new[] { "n1", "n2", "n3" } ) );
            Assert.That( document.Graph.Edges.Select( e => e.Id ), Is.EquivalentTo( new[] { Edge.CreateId( "n1", "n2" ), Edge.CreateId( "n2", "n3" ) } ) );
        }

        [Test]
        public void Read_SimpleDirectedGraphWithTwoEdgesAndSemiColon_NotesDetected()
        {
            var document = new DotLangPureDocument();

            using( var reader = new StringReader( @"digraph { n1 -> n2; n2 -> n3; }" ) )
            {
                document.Read( reader );
            }

            Assert.That( document.Graph.Nodes.Select( n => n.Id ), Is.EquivalentTo( new[] { "n1", "n2", "n3" } ) );
            Assert.That( document.Graph.Edges.Select( e => e.Id ), Is.EquivalentTo( new[] { Edge.CreateId( "n1", "n2" ), Edge.CreateId( "n2", "n3" ) } ) );
        }

        [Test]
        public void Read_EdgesWithQuotes_NotesDetected()
        {
            var document = new DotLangPureDocument();

            using( var reader = new StringReader( "digraph { \"Node 1\" -> \"Node 2\"; }" ) )
            {
                document.Read( reader );
            }

            Assert.That( document.Graph.Nodes.Select( n => n.Id ), Is.EquivalentTo( new[] { "Node 1", "Node 2" } ) );
            Assert.That( document.Graph.Edges.Select( e => e.Id ), Is.EquivalentTo( new[] { Edge.CreateId( "Node 1", "Node 2" ) } ) );
        }

        [Test]
        public void Read_NodeWithLabel_NodeAndLabelDetected()
        {
            var document = new DotLangPureDocument();

            using( var reader = new StringReader( "graph { n1 [ label= \"Another text\" ]; n2; n3 }" ) )
            {
                document.Read( reader );
            }

            Assert.That( document.Graph.Nodes.Select( n => n.Id ), Is.EquivalentTo( new[] { "n1", "n2", "n3" } ) );
            Assert.That( document.Captions.Single( c => c.OwnerId == "n1" ).Label, Is.EqualTo( "Another text" ) );
        }

        [Test]
        public void Read_EdgeWithLabel_EdgeAndLabelDetected()
        {
            var document = new DotLangPureDocument();

            using( var reader = new StringReader( "digraph { n1 -> n2 [ label= \"Implemented by\" ]; n2 -> n3 [ label=\"called by\"] }" ) )
            {
                document.Read( reader );
            }

            var edge1 = Edge.CreateId( "n1", "n2" );
            var edge2 = Edge.CreateId( "n2", "n3" );
            Assert.That( document.Graph.Edges.Select( e => e.Id ), Is.EquivalentTo( new[] { edge1, edge2 } ) );
            Assert.That( document.Captions.Single( c => c.OwnerId == edge1 ).Label, Is.EqualTo( "Implemented by" ) );
            Assert.That( document.Captions.Single( c => c.OwnerId == edge2 ).Label, Is.EqualTo( "called by" ) );
        }
    }
}
