using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Plainion.GraphViz.Model;
using Plainion.GraphViz.Presentation;

namespace Plainion.GraphViz.Dot
{
    public class DotToolLayoutEngine : ILayoutEngine
    {
        private FileInfo myDotFile;
        private FileInfo myPlainFile;
        private DotToDotPlainConverter myConverter;

        public DotToolLayoutEngine( DotToDotPlainConverter converter )
        {
            myConverter = converter;

            var sessionId = Guid.NewGuid().ToString();
            myDotFile = new FileInfo( Path.Combine( Path.GetTempPath(), sessionId + ".dot" ) );
            myPlainFile = new FileInfo( Path.Combine( Path.GetTempPath(), sessionId + ".plain" ) );
        }

        public void Relayout( IGraphPresentation presentation )
        {
            GenerateDotFile( presentation );

            myConverter.Convert( myDotFile, myPlainFile );

            var nodeLayouts = new List<NodeLayout>();
            var edgeLayouts = new List<EdgeLayout>();

            ParsePlainFile( nodeLayouts, edgeLayouts, presentation.GetPropertySetFor<Caption>() );

            var module = presentation.GetModule<IGraphLayoutModule>();
            module.Set( nodeLayouts, edgeLayouts );
        }

        // TODO: we have to consider StyleModule
        private void GenerateDotFile( IGraphPresentation presentation )
        {
            var labelModule = presentation.GetPropertySetFor<Caption>();
            using( var writer = new StreamWriter( myDotFile.FullName ) )
            {
                writer.WriteLine( "digraph {" );
                writer.WriteLine( "  ratio=\"compress\"" );
                writer.WriteLine( "  rankdir=BT" );
                writer.WriteLine( "  ranksep=\"2.0 equally\"" );

                foreach( var node in presentation.Graph.Nodes.Where( n => presentation.Picking.Pick( n ) ) )
                {
                    // pass label to trigger dot.exe to create proper size of node bounding box
                    var label = labelModule.Get( node.Id ).DisplayText;

                    writer.WriteLine( "  \"{0}\" [label=\"{1}\"]", node.Id, label );
                }

                foreach( var edge in presentation.Graph.Edges.Where( e => presentation.Picking.Pick( e ) ) )
                {
                    // pass label to trigger dot.exe to create position of the label
                    var label = labelModule.Get( edge.Id );

                    // always pass label otherwise parser will fail :(
                    writer.WriteLine( "  \"{0}\" -> \"{1}\" [label=\"{2}\"]", edge.Source.Id, edge.Target.Id, label.DisplayText != label.OwnerId ? label.DisplayText : "." );
                }

                writer.WriteLine( "}" );
            }
        }

        private void ParsePlainFile( List<NodeLayout> nodeLayouts, List<EdgeLayout> edgeLayouts, IPropertySetModule<Caption> captionModule )
        {
            using( var reader = new DotPlainReader( new StreamReader( myPlainFile.FullName ) ) )
            {
                var parser = new DotPlainParser( reader );

                parser.Open();

                while( parser.MoveNextEntry( "node" ) )
                {
                    var nodeId = parser.ReadId();

                    var layout = parser.ReadNodeLayout( nodeId );
                    nodeLayouts.Add( layout );
                }

                while( parser.MoveNextEntry( "edge" ) )
                {
                    var sourceNodeId = parser.ReadId();
                    var targetNodeId = parser.ReadId();

                    var edgeId = Edge.CreateId( sourceNodeId, targetNodeId );

                    var layout = parser.ReadEdgeLayout( edgeId );
                    var label = parser.ReadLabel( edgeId );
                    layout.LabelPosition = parser.ReadPoint();
                    edgeLayouts.Add( layout );
                }
            }
        }
    }
}
