using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Windows.Data;

namespace Plainion.GraphViz.Modules.Analysis.Clusters;

class ShowChildrenCountConverter : IMultiValueConverter
{
    public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
    {
        var owner = (NodeItem)values[0];
        var children = (IEnumerable<ClusterTreeNode>)values[1];

        if (owner.State == null || !owner.State.ShowChildrenCount)
        {
            return null;
        }

        var childrenCount = children.Count();
        return childrenCount > 0
            ? string.Format("[{0}]", childrenCount)
            : null;
    }

    public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}
