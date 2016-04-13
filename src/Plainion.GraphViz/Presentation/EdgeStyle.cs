using System.Windows.Media;

namespace Plainion.GraphViz.Presentation
{
    public class EdgeStyle : AbstractStyle
    {
        private Brush myColor;

        public EdgeStyle(string ownerId)
            : base(ownerId)
        {
            Color = Brushes.Black;
            Style = "solid";
        }

        public Brush Color
        {
            get { return myColor; }
            set { SetProperty(ref myColor, value); }
        }
    }
}
