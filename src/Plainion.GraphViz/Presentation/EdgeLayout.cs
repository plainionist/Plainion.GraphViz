using System.Collections.Generic;
using System.Linq;
using System.Windows;

namespace Plainion.GraphViz.Presentation
{
    public class EdgeLayout : AbstractPropertySet
    {
        private IEnumerable<Point> myPoints;
        private Point myLabelPosition;

        public EdgeLayout( string ownerId )
            : base( ownerId )
        {
            Points = null;
        }

        public IEnumerable<Point> Points
        {
            get { return myPoints; }
            set { SetProperty( ref myPoints, value != null ? value.ToList() : Enumerable.Empty<Point>(), "Points" ); }
        }

        public Point LabelPosition
        {
            get { return myLabelPosition; }
            set { SetProperty( ref myLabelPosition, value, "LabelPosition" ); }
        }
    }
}
