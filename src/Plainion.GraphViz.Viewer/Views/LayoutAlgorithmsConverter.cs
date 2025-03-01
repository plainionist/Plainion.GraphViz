
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
            if (value == null)
            {
                // this e.g. happens when graph is loaded and RDP connection is opened to this machine
                return "Auto";
            }

            var algo = (LayoutAlgorithm)value;
            if (algo == LayoutAlgorithm.Hierarchy)
            {
                return "Trees";
            }
            else if (algo == LayoutAlgorithm.Flow)
            {
                return "Flow";
            }
            else if (algo == LayoutAlgorithm.ScalableForceDirectedPlancement)
            {
                return "Large Galaxies";
            }
            else if (algo == LayoutAlgorithm.ForceDirectedPlacement)
            {
                return "Small Galaxies";
            }
            else if (algo == LayoutAlgorithm.NeatSpring)
            {
                return "Organic";
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
