using System;
using System.Linq;
using Plainion.Windows.Controls.Tree;

namespace Plainion.GraphViz.Modules.Analysis
{
    static class TreeExtensions
    {
        public static bool Any<T>(this T root, Func<T, bool> predicate) where T : INode
        {
            if (predicate(root))
            {
                return true;
            }

            return Enumerable.Any(root.Children.OfType<T>(), n => n.Any(predicate));
        }
    }
}
