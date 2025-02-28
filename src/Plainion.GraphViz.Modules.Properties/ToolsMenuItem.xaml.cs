using System.Windows.Controls;

namespace Plainion.GraphViz.Modules.Properties;

partial class ToolsMenuItem : MenuItem
{
    public ToolsMenuItem(ToolsMenuItemModel model)
    {
        InitializeComponent();

        DataContext = model;
    }
}
