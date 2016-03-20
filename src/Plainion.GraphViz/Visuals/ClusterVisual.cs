using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Windows;
using System.Windows.Media;
using Plainion.GraphViz.Model;
using Plainion.GraphViz.Presentation;

namespace Plainion.GraphViz.Visuals
{
    internal class ClusterVisual : AbstractElementVisual
    {
        private static Typeface myFont;

        private const double BorderThickness = 0.032;
        private const double FontSize = BorderThickness * 10;

        private IGraphPresentation myPresentation;

        static ClusterVisual()
        {
            myFont = new Typeface( "Verdana" );
        }

        public ClusterVisual( Cluster owner, IGraphPresentation presentation )
        {
            Owner = owner;
            myPresentation = presentation;
        }

        public Cluster Owner { get; private set; }

        public void Draw( IDictionary<string, AbstractElementVisual> drawingElements )
        {
            var label = myPresentation.GetPropertySetFor<Caption>().Get( Owner.Id );

            Visual = new DrawingVisual();
            var dc = Visual.RenderOpen();

            var rect = GetBoundingBox( drawingElements );
            dc.DrawRectangle( Brushes.Transparent, new Pen( Brushes.Blue, BorderThickness ), rect );

            var tx = new FormattedText( label.DisplayText,
                  CultureInfo.InvariantCulture,
                  FlowDirection.LeftToRight,
                  myFont,
                  FontSize, Brushes.Black );

            var fontPadding = BorderThickness * 3;
            dc.DrawText( tx, new Point( rect.Left + fontPadding, rect.Top + fontPadding ) );

            dc.Close();

            Visual.SetValue( GraphItemProperty, Owner );
        }

        private Rect GetBoundingBox( IDictionary<string, AbstractElementVisual> drawingElements )
        {
            // 1. include all visible nodes within this cluster
            var visibleNodes = Owner.Nodes
                .Where( n => myPresentation.Picking.Pick( n ) )
                .ToList();

            var box = drawingElements[ visibleNodes.First().Id ].Visual.ContentBounds;

            foreach( var node in visibleNodes.Skip( 1 ) )
            {
                var nodeBox = drawingElements[ node.Id ].Visual.ContentBounds;
                box.Union( nodeBox );
            }

            // 2. include all visible edges which have source and target within this cluster
            var transformationModule = myPresentation.GetModule<ITransformationModule>();
            var visibleEdges = transformationModule.Graph.Edges
                .Where( e => visibleNodes.Contains( e.Source ) && visibleNodes.Contains( e.Target ) )
                .Where( e => myPresentation.Picking.Pick( e ) )
                .ToList();

            foreach( var edge in visibleEdges )
            {
                var nodeBox = drawingElements[ edge.Id ].Visual.ContentBounds;
                box.Union( nodeBox );
            }

            // 3. add some padding for label
            box = new Rect( box.Left, box.Top - FontSize * 2, box.Width, box.Height + FontSize * 2 );

            // 4. add some padding
            box.Inflate( BorderThickness * 7, BorderThickness * 7 );

            return box;
        }
    }
}
