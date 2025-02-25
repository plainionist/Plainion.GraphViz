using System.Windows;
using System.Windows.Media;

namespace Plainion.GraphViz.Modules.Analysis.Clusters;

static class VisualTreeUtils
{
    public static T FindParentOfType<T>(this DependencyObject self) where T : DependencyObject
    {
        while (self != null && self is not T)
        {
            self = VisualTreeHelper.GetParent(self);
        }
        return (T)self;
    }
}
