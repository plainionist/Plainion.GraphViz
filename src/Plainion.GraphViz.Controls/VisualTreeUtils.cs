using System.Windows;
using System.Windows.Media;

namespace Plainion.GraphViz.Controls;

static class VisualTreeUtils
{
    public static T FindParentOfType<T>(this DependencyObject self) where T : DependencyObject
    {
        while (self != null && !(self is T))
        {
            self = VisualTreeHelper.GetParent(self);
        }
        return (T)self;
    }
}
