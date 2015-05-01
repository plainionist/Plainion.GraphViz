using System.Windows;

namespace Plainion.GraphViz.Presentation
{
    public class NodeLayout : AbstractPropertySet
    {
        private Point myCenter;
        private double myWidth;
        private double myHeight;

        public NodeLayout( string ownerId )
            :base(ownerId)
        {
        }

        public Point Center
        {
            get { return myCenter; }
            set { SetProperty( ref myCenter, value, "Center" ); }
        }

        public double Width
        {
            get { return myWidth; }
            set { SetProperty( ref myWidth, value, "Width" ); }
        }

        public double Height
        {
            get { return myHeight; }
            set { SetProperty( ref myHeight, value, "Height" ); }
        }
    }
}
