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
            var caption = myPresentation.GetPropertySetFor<Caption>().Get( Owner.Id );

            Visual = new DrawingVisual();
            var dc = Visual.RenderOpen();

            var tx = new FormattedText( caption.DisplayText,
                  CultureInfo.InvariantCulture,
                  FlowDirection.LeftToRight,
                  myFont,
                  FontSize, Brushes.Black );

            const double FontPadding = BorderThickness * 3;

            var rect = GetBoundingBox( drawingElements );

            // resize rect so that cluster caption can be drawn
            rect = new Rect(
                rect.Left - FontPadding,
                rect.Top - ( tx.Height + 2 * FontPadding ),
                Math.Max( rect.Width, tx.Width ) + 2 * FontPadding,
                rect.Height + tx.Height + 3 * FontPadding );

            // add some extra padding
            const double ExtraPadding = BorderThickness * 3;
            rect.Inflate( ExtraPadding, ExtraPadding );

            dc.DrawRectangle( Brushes.Transparent, new Pen( Brushes.Blue, BorderThickness ), rect );

            dc.DrawText( tx, new Point( rect.Left + FontPadding, rect.Top + FontPadding ) );

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

            return box;
        }

        protected override Brush GetBorderBrush()
        {
            return Brushes.Blue;
        }
    }
}
