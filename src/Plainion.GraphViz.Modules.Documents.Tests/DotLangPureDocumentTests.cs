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
            var document = new DotLangDocument();

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
            var document = new DotLangDocument();

            using( var reader = new StringReader( @"digraph { n1; n2; }" ) )
            {
                document.Read( reader );
            }

            Assert.That( document.Graph.Nodes.Select( n => n.Id ), Is.EquivalentTo( new[] { "n1", "n2" } ) );
        }

        [Test]
        public void Read_NodeIdWithQotes_NotesDetected()
        {
            var document = new DotLangDocument();

            using( var reader = new StringReader( "digraph { \"Node 1\"; \"Node 2\"; }" ) )
            {
                document.Read( reader );
            }

            Assert.That( document.Graph.Nodes.Select( n => n.Id ), Is.EquivalentTo( new[] { "Node 1", "Node 2" } ) );
        }

        [Test]
        public void Read_CPlusPlusStyleComment_Ignored()
        {
            var document = new DotLangDocument();

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
            var document = new DotLangDocument();

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
            var document = new DotLangDocument();

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
            var document = new DotLangDocument();

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
            var document = new DotLangDocument();

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
            var document = new DotLangDocument();

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
            var document = new DotLangDocument();

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

        [Test]
        public void Read_WithSubgraphs_ClustersDetected()
        {
            var document = new DotLangDocument();

            using( var reader = new StringReader( "digraph {   subgraph cluster_C1 { label=\"C1\"; a0; a1; }    subgraph cluster_C2 { label=\"C2\"; b0 -> b1; } a0 -> b0; }" ) )
            {
                document.Read( reader );
            }

            {
                var edge1 = Edge.CreateId( "b0", "b1" );
                var edge2 = Edge.CreateId( "a0", "b0" );
                Assert.That( document.Graph.Edges.Select( e => e.Id ), Is.EquivalentTo( new[] { edge1, edge2 } ) );
            }

            {
                var c1 = document.Graph.Clusters.SingleOrDefault( c => c.Id == "cluster_C1" );
                Assert.That( c1, Is.Not.Null, "Cluster C1 not found" );

                Assert.That( document.Captions.Single( c => c.OwnerId == "cluster_C1" ).Label, Is.EqualTo( "C1" ) );

                Assert.That( c1.Nodes.Select( n => n.Id ), Is.EqualTo( new[] { "a0", "a1" } ) );
            }

            {
                var c2 = document.Graph.Clusters.SingleOrDefault( c => c.Id == "cluster_C2" );
                Assert.That( c2, Is.Not.Null, "Cluster C2 not found" );

                Assert.That( document.Captions.Single( c => c.OwnerId == "cluster_C2" ).Label, Is.EqualTo( "C2" ) );

                Assert.That( c2.Nodes.Select( n => n.Id ), Is.EqualTo( new[] { "b0", "b1" } ) );
            }
        }
    }
}
