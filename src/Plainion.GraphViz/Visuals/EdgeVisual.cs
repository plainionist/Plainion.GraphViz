using System.Linq;
using System.Windows.Media;
using Plainion.GraphViz.Model;
using Plainion.GraphViz.Presentation;

namespace Plainion.GraphViz.Visuals
{
    internal class EdgeVisual : AbstractElementVisual
    {
        private IGraphPresentation myPresentation;

        public EdgeVisual( Edge owner, IGraphPresentation presentation )
        {
            Owner = owner;
            myPresentation = presentation;
        }

        public Edge Owner
        {
            get;
            private set;
        }

        public void Draw( EdgeLayout layoutState )
        {
            var styleState = myPresentation.GetPropertySetFor<EdgeStyle>().Get( Owner.Id );

            var stream = new StreamGeometry();
            var context = stream.Open();

            context.BeginFigure( layoutState.Points.First(), false, false );

            context.PolyBezierTo( layoutState.Points.Skip( 1 ).ToList(), true, false );

            // draw arrow head
            var start = layoutState.Points.Last();
            var v = start - layoutState.Points.ElementAt( layoutState.Points.Count() - 2 );
            v.Normalize();

            start = start - v * 0.15;
            context.BeginFigure( start + v * 0.28, true, true );
            double t = v.X; v.X = v.Y; v.Y = -t;  // Rotate 90°
            context.LineTo( start + v * 0.08, true, true );
            context.LineTo( start + v * -0.08, true, true );
            context.Close();

            var pen = new Pen( styleState.Color, 0.016 );

            // http://stackoverflow.com/questions/1755520/improve-drawingvisual-renders-speed
            Visual = new DrawingVisual();
            var dc = Visual.RenderOpen();
            dc.DrawGeometry( pen.Brush, pen, stream );
            dc.Close();

            Visual.SetValue( GraphItemProperty, Owner );
        }
    }
}
