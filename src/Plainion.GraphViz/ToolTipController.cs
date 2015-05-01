using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Threading;

namespace Plainion.GraphViz
{
    internal class ToolTipController
    {
        private object myCurrentContent;
        private string myCurrentDrawingId;
        private ToolTip myTip;
        private Point myInitialPosition;
        private DispatcherTimer myTimer;

        public ToolTipController()
        {
            myTip = new ToolTip();
            myTimer = new DispatcherTimer();
            myTimer.Interval = new TimeSpan( ToolTipService.GetInitialShowDelay( myTip ) * 10000 );
            myTimer.Tick += new EventHandler( OnTick );
        }

        public void Move( string drawingId, object content )
        {
            if( content != myCurrentContent || drawingId != myCurrentDrawingId )
            {
                myTimer.Stop();

                myCurrentContent = content;
                myCurrentDrawingId = drawingId;

                if( myTip.IsOpen )
                {
                    Hide();
                }

                myTimer.Start();
            }
            else if( myTip.IsOpen )
            {
                var delta = Mouse.GetPosition( null ) - myInitialPosition;

                myTip.VerticalOffset = delta.Y;
                myTip.HorizontalOffset = delta.X;
            }
        }

        public void Hide()
        {
            myTimer.Stop();

            myCurrentDrawingId = null;
            myCurrentContent = null;

            myTip.IsOpen = false;
        }

        private void OnTick( object sender, EventArgs e )
        {
            myTimer.Stop();

            if( myCurrentContent != null )
            {
                myTip.VerticalOffset = 0;
                myTip.HorizontalOffset = 0;
                myTip.Content = myCurrentContent;
                myTip.IsOpen = myTip.Content != null;

                myInitialPosition = Mouse.GetPosition( null );
            }
        }
    }
}
