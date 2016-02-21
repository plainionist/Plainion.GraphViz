using System.Windows;
using System.Windows.Media;

namespace Plainion.GraphViz.Presentation
{
    public class NodeStyle : AbstractStyle
    {
        private Brush myFillColor;
        private Brush myBorderColor;
        private string myShape;

        public NodeStyle( string ownerId )
            : base( ownerId )
        {
        }

        public Brush FillColor
        {
            get { return myFillColor; }
            set { SetProperty( ref myFillColor, value ); }
        }

        // e.g. ellipse
        public string Shape
        {
            get { return myShape; }
            set { SetProperty( ref myShape, value ); }
        }

        public Brush BorderColor
        {
            get { return myBorderColor; }
            set { SetProperty( ref myBorderColor, value ); }
        }
    }
}
