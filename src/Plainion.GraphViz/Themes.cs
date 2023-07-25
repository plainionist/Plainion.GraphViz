using System.Windows.Media;

namespace Plainion.GraphViz
{
    public class Themes
    {
        public static Theme Current => new DarkTheme();

        private class LightTheme : Theme
        {
            public LightTheme()
            {
                WindowBackground = (Brush)new BrushConverter().ConvertFrom("#FDFDF5");
            }

            public override Brush EdgeBrush(Brush color) => color;
            public override Brush NodeBorderColor(Brush color) => color;
            public override Brush NodeFillColor(Brush color) => color;
            public override Brush NodeCaptionColor(Brush color) => color;
            public override Brush CaptionColor(Brush color) => color;
            public override Brush ClusterBorderColor(Brush color) => color;
        }

        private class DarkTheme : Theme
        {
            public DarkTheme()
            {
                WindowBackground = (Brush)new BrushConverter().ConvertFrom("#1E1E1E");
            }

            public override Brush EdgeBrush(Brush color) => Brushes.WhiteSmoke;
            public override Brush NodeBorderColor(Brush color) => Brushes.DarkGray;
            public override Brush NodeFillColor(Brush color) => Brushes.LightGray;
            public override Brush NodeCaptionColor(Brush color) => Brushes.Black;
            public override Brush CaptionColor(Brush color) => Brushes.LightGray;
            public override Brush ClusterBorderColor(Brush color) => Brushes.DodgerBlue;
        }
    }

    public abstract class Theme
    {
        public Brush WindowBackground { get; protected set; }

        public abstract Brush EdgeBrush(Brush color);
        public abstract Brush NodeBorderColor(Brush color);
        public abstract Brush NodeFillColor(Brush color);
        public abstract Brush NodeCaptionColor(Brush color);
        public abstract Brush CaptionColor(Brush color);
        public abstract Brush ClusterBorderColor(Brush color);
    }
}
