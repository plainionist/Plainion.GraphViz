using System.Collections.Generic;
using System.Windows;
using Plainion.GraphViz.Presentation;
using System;
using Plainion;

namespace Plainion.GraphViz.Dot
{
    public class DotPlainParser
    {
        private DotPlainReader myReader;

        public DotPlainParser( DotPlainReader reader )
        {
            Contract.RequiresNotNull( reader, "reader" );

            myReader = reader;
        }

        public DotGraphHeader Header { get; private set; }

        public void Open()
        {
            if( MoveNextEntry( "graph" ) )
            {
                Header = new DotGraphHeader();
                Header.Scale = myReader.ReadInt();
                Header.Width = myReader.ReadDouble();
                Header.Height = myReader.ReadDouble();
            }
            else
            {
                throw new InvalidOperationException( "First line should start with 'graph'" );
            }
        }

        public bool MoveNextEntry( string tag )
        {
            return myReader.ReadLine( tag );
        }

        public Point ReadPoint()
        {
            // the dot coordinate origin is in the lower left, wpf is in the upper left
            return new Point( myReader.ReadDouble(), Header.Height - myReader.ReadDouble() );
        }

        public NodeLayout ReadNodeLayout( string nodeId )
        {
            var layout = new NodeLayout( nodeId );

            layout.Center = ReadPoint();
            layout.Width = myReader.ReadDouble() / 2;
            layout.Height = myReader.ReadDouble() / 2;

            return layout;
        }

        public Caption ReadLabel( string nodeId )
        {
            var label = myReader.ReadString();
            return new Caption( nodeId, label );
        }

        public NodeStyle ReadNodeStyle( string nodeId )
        {
            var style = new NodeStyle( nodeId );
            style.Style = myReader.ReadString();
            style.Shape = myReader.ReadString();
            style.BorderColor = StyleConverter.GetBrush( myReader.ReadString() );
            style.FillColor = StyleConverter.GetBrush( myReader.ReadString() );

            return style;
        }

        public string ReadId()
        {
            return myReader.ReadString();
        }

        public EdgeLayout ReadEdgeLayout( string edgeId )
        {
            int pointCount = myReader.ReadInt();
            var points = new List<Point>();
            for( int i = 0; i < pointCount; ++i )
            {
                points.Add( ReadPoint() );
            }

            var layout = new EdgeLayout( edgeId );
            layout.Points = points;

            return layout;
        }

        public EdgeStyle ReadEdgeStyle( string edgeId )
        {
            var style = new EdgeStyle( edgeId );
            style.Style = myReader.ReadString();
            style.Color = StyleConverter.GetBrush( myReader.ReadString() );

            return style;
        }
    }
}
