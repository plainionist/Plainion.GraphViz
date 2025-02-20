using System.Windows.Controls;

namespace Plainion.GraphViz.Modules.Obfuscate;

partial class ToolsMenuItem : MenuItem
{
    public ToolsMenuItem(ToolsMenuItemModel model)
    {
        InitializeComponent();

        DataContext = model;
    }
}
