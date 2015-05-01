using System;
using System.Windows;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;

namespace Plainion.GraphViz
{
    internal class RubberbandAdorner : Adorner
    {
        private Point myStartPoint;
        private Point? myEndPoint;
        private Pen myPen;
        private UIElement myOwner;
        private Func<MouseEventArgs, MouseButtonState> myMouseButtonSelector;

        public RubberbandAdorner( UIElement owner, Point startPoint, Func<MouseEventArgs,MouseButtonState> mouseButtonSelector  )
            : base( owner )
        {
            myOwner = owner;
            myStartPoint = startPoint;
            myMouseButtonSelector = mouseButtonSelector;

            myPen = new Pen( Brushes.LightSlateGray, 1 );
            myPen.DashStyle = new DashStyle( new double[] { 2 }, 1 );
        }

        protected override void OnMouseMove( MouseEventArgs e )
        {
            if( e.RightButton == MouseButtonState.Pressed )
            {
                if( !IsMouseCaptured )
                {
                    CaptureMouse();
                }

                myEndPoint = e.GetPosition( this );
                InvalidateVisual();
            }
            else
            {
                if( IsMouseCaptured )
                {
                    ReleaseMouseCapture();
                }
            }

            e.Handled = true;
        }

        protected override void OnMouseUp( MouseButtonEventArgs e )
        {
            if( IsMouseCaptured )
            {
                ReleaseMouseCapture();
            }

            var adornerLayer = AdornerLayer.GetAdornerLayer( myOwner );
            if( adornerLayer != null )
            {
                adornerLayer.Remove( this );
            }

            if( Closed != null )
            {
                Closed( this, EventArgs.Empty );
            }
        }

        protected override void OnRender( DrawingContext dc )
        {
            base.OnRender( dc );

            // without a background the OnMouseMove event would not be fired!
            // Alternative: implement a Canvas as a child of this adorner, like
            // the ConnectionAdorner does.
            dc.DrawRectangle( Brushes.Transparent, null, new Rect( RenderSize ) );

            if( myEndPoint.HasValue )
            {
                dc.DrawRectangle( Brushes.Transparent, myPen, new Rect( myStartPoint, myEndPoint.Value ) );
            }
        }

        public event EventHandler Closed;
    }
}
