using System.Windows;
using System.Windows.Documents;
using System.Windows.Media;

namespace Plainion.GraphViz
{
    class GraphPaginator : DocumentPaginator
    {
        // margin 1.5 cm
        private const double Margin = 1.5 * 96 / 2.54;

        private Drawing myGraph;
        private Size contentSize; // area that will be printed with graph content
        private Rect myFrameRect;   // rectangle drawn around the content size
        private Pen myFramePen;
        private int myPageCountX;  // Size of the printing in page units
        private int myPageCountY;

        public GraphPaginator( DrawingVisual source, Size printSize )
        {
            PageSize = printSize;

            contentSize = new Size( printSize.Width - 2 * Margin, printSize.Height - 2 * Margin );

            myFrameRect = new Rect( new Point( Margin, Margin ), contentSize );
            myFrameRect.Inflate( 1, 1 );
            myFramePen = new Pen( Brushes.Black, 0.1 );

            // Transformation to borderless print size
            var bounds = source.DescendantBounds;
            bounds.Union( source.ContentBounds );

            var m = new Matrix();
            m.Translate( -bounds.Left, -bounds.Top );

            double scale = 16; // hardcoded zoom for printing
            myPageCountX = ( int )( ( bounds.Width * scale ) / contentSize.Width ) + 1;
            myPageCountY = ( int )( ( bounds.Height * scale ) / contentSize.Height ) + 1;
            m.Scale( scale, scale );
        
            // Center on available pages
            m.Translate( ( myPageCountX * contentSize.Width - bounds.Width * scale ) / 2, ( myPageCountY * contentSize.Height - bounds.Height * scale ) / 2 );

            var target = new DrawingVisual();
            using( var dc = target.RenderOpen() )
            {
                dc.PushTransform( new MatrixTransform( m ) );
                dc.DrawDrawing( source.Drawing );

                foreach( DrawingVisual child in source.Children )
                {
                    dc.DrawDrawing( child.Drawing );
                }
            }
            myGraph = target.Drawing;
        }

        public override DocumentPage GetPage( int pageNumber )
        {
            int x = pageNumber % myPageCountX;
            int y = pageNumber / myPageCountX;

            var view = new Rect();
            view.X = x * contentSize.Width;
            view.Y = y * contentSize.Height;
            view.Size = contentSize;

            var targetVisual = new DrawingVisual();
            using( var dc = targetVisual.RenderOpen() )
            {
                dc.DrawRectangle( null, myFramePen, myFrameRect );
                dc.PushTransform( new TranslateTransform( Margin - view.X, Margin - view.Y ) );
                dc.PushClip( new RectangleGeometry( view ) );
               
                dc.DrawDrawing( myGraph );
            }

            return new DocumentPage( targetVisual, PageSize, myFrameRect, myFrameRect );
        }

        public override bool IsPageCountValid
        {
            get
            {
                return true;
            }
        }

        public override int PageCount
        {
            get
            {
                return myPageCountY * myPageCountX;
            }
        }

        public override Size PageSize
        {
            get;
            set;
        }

        public override IDocumentPaginatorSource Source
        {
            get { return null; }
        }
    }
}