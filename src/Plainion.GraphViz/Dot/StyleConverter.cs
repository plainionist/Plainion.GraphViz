using System;
using System.Collections.Generic;
using System.Windows.Media;

namespace Plainion.GraphViz.Dot
{
    // TODO: move from singleton to DI
    public class StyleConverter
    {
        private static BrushConverter myBrushConverter = new BrushConverter();
        private static Dictionary<string, Brush> myBrushes = new Dictionary<string, Brush>();

        public static Brush GetBrush( string color )
        {
            Brush brush;
            if( !myBrushes.TryGetValue( color, out brush ) )
            {
                try
                {
                    if( color.Equals( "lightgrey", StringComparison.OrdinalIgnoreCase ) )
                    {
                        color = "LightGray";
                    }

                    brush = ( Brush )myBrushConverter.ConvertFromInvariantString( color );
                    brush.Freeze();
                }
                catch( FormatException )
                {
                    brush = Brushes.Black;
                }
            }
            return brush;
        }
    }
}
