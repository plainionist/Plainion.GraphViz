
using System;
using System.Globalization;
using System.Windows.Data;
using Plainion.GraphViz.Dot;

namespace Plainion.GraphViz.Viewer.Views
{
    class LayoutAlgorithmsConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var algo = (LayoutAlgorithm)value;
            if (algo == LayoutAlgorithm.Auto)
            {
                return "Auto";
            }
            else if (algo == LayoutAlgorithm.Hierarchy)
            {
                return "Trees";
            }
            else if (algo == LayoutAlgorithm.Flow)
            {
                return "Flow";
            }
            else if (algo == LayoutAlgorithm.Sfdp)
            {
                return "Galaxies";
            }
            else 
            {
                return algo.ToString();
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
