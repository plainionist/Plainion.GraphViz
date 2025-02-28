using System.Windows.Controls;
using Prism.Navigation.Regions;

namespace Plainion.GraphViz.Modules.Obfuscate;

[ViewSortHint(Viewer.Abstractions.RegionNames.AddIns + ".1000")]
partial class ToolsMenuItem : MenuItem
{
    public ToolsMenuItem(ToolsMenuItemModel model)
    {
        InitializeComponent();

        DataContext = model;
    }
}
