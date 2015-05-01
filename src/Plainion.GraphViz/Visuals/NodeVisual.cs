using System.Globalization;
using System.Windows;
using System.Windows.Media;
using Plainion.GraphViz.Model;
using Plainion.GraphViz.Presentation;

namespace Plainion.GraphViz.Visuals
{
    internal class NodeVisual : AbstractElementVisual
    {
        private static Typeface myFont;

        private IGraphPresentation myPresentation;

        static NodeVisual()
        {
            myFont = new Typeface( "Verdana" );
        }

        public NodeVisual( Node owner, IGraphPresentation presentation )
        {
            Owner = owner;
            myPresentation = presentation;
        }

        public Node Owner
        {
            get;
            private set;
        }

        // TODO: we should interpret the shape/style/color attributes ...
        public void Draw( NodeLayout layoutState )
        {
            var style = myPresentation.GetPropertySetFor<NodeStyle>().Get( Owner.Id );
            var label = myPresentation.GetPropertySetFor<Caption>().Get( Owner.Id );

            Visual = new DrawingVisual();
            var dc = Visual.RenderOpen();

            dc.DrawEllipse( style.FillColor, new Pen( style.BorderColor, 0.016 ), layoutState.Center, layoutState.Width, layoutState.Height );

            var tx = new FormattedText( label.DisplayText,
                  CultureInfo.InvariantCulture,
                  FlowDirection.LeftToRight,
                  myFont,
                  layoutState.Height * 0.7, Brushes.Black );

            dc.DrawText( tx, new Point( layoutState.Center.X - tx.Width / 2, layoutState.Center.Y - tx.Height / 2 ) );

            dc.Close();

            Visual.SetValue( GraphItemProperty, Owner );
        }
    }
}
