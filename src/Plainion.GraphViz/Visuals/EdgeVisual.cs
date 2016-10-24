using System.Globalization;
using System.Linq;
using System.Windows;
using System.Windows.Media;
using Plainion.GraphViz.Model;
using Plainion.GraphViz.Presentation;

namespace Plainion.GraphViz.Visuals
{
    internal class EdgeVisual : AbstractElementVisual
    {
        private static Typeface myFont;

        private IGraphPresentation myPresentation;
        private double myZoomFactor;

        static EdgeVisual()
        {
            myFont = new Typeface( "Verdana" );
        }

        public EdgeVisual( Edge owner, IGraphPresentation presentation, double zoomFactor )
        {
            Owner = owner;
            myPresentation = presentation;

            myZoomFactor = zoomFactor;
        }

        public Edge Owner { get; private set; }

        public void Draw( EdgeLayout layoutState )
        {
            var styleState = myPresentation.GetPropertySetFor<EdgeStyle>().Get( Owner.Id );
            var label = myPresentation.GetPropertySetFor<Caption>().Get( Owner.Id );

            var stream = new StreamGeometry();
            var context = stream.Open();

            context.BeginFigure( layoutState.Points.First(), false, false );

            context.PolyBezierTo( layoutState.Points.Skip( 1 ).ToList(), true, false );

            // draw arrow head
            {
                var start = layoutState.Points.Last();
                var v = start - layoutState.Points.ElementAt( layoutState.Points.Count() - 2 );
                v.Normalize();

                start = start - v * 0.05;
                context.BeginFigure( start + v * 0.18, true, true );

                // Rotate 90°
                double t = v.X;
                v.X = v.Y;
                v.Y = -t;

                context.LineTo( start + v * 0.06, true, true );
                context.LineTo( start + v * -0.06, true, true );

                context.Close();
            }

            var pen = new Pen( styleState.Color, 1 );
            SetLineThickness( pen );

            // http://stackoverflow.com/questions/1755520/improve-drawingvisual-renders-speed
            Visual = new DrawingVisual();
            var dc = Visual.RenderOpen();
            dc.DrawGeometry( pen.Brush, pen, stream );

            if( label.DisplayText != label.OwnerId )
            {
                var sourceLayoutState = myPresentation.GetModule<IGraphLayoutModule>().GetLayout( Owner.Source );

                var tx = new FormattedText( label.DisplayText,
                    CultureInfo.InvariantCulture,
                    FlowDirection.LeftToRight,
                    myFont,
                    sourceLayoutState.Height * 0.5, Brushes.Black );

                dc.DrawText( tx, new Point( layoutState.LabelPosition.X - tx.Width, layoutState.LabelPosition.Y - tx.Height ) );
            }

            dc.Close();

            Visual.SetValue( GraphItemProperty, Owner );
        }

        protected override Brush GetBorderBrush()
        {
            var style = myPresentation.GetPropertySetFor<EdgeStyle>().Get( Owner.Id );
            return style.Color;
        }

        public void ApplyZoomFactor( double zoomFactor )
        {
            if( Visual == null )
            {
                return;
            }

            myZoomFactor = zoomFactor;

            ApplyToGeometries( Visual.Drawing, g => SetLineThickness( g.Pen ) );
        }

        private void SetLineThickness( Pen pen )
        {
            // make lines thicker if we zoom out so that we can still see them
            pen.Thickness = 0.016 * 0.5 / myZoomFactor;
        }
    }
}
