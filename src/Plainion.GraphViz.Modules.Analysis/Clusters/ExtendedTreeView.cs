using System.Windows;
using System.Windows.Controls;

namespace Plainion.GraphViz.Modules.Analysis.Clusters;

class ExtendedTreeView : TreeView
{
    protected override DependencyObject GetContainerForItemOverride() => new NodeView();

    protected override bool IsItemItsOwnContainerOverride(object item) => item is NodeView;
}
