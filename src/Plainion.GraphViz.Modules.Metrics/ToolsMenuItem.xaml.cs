using System.Windows.Controls;
using Prism.Navigation.Regions;

namespace Plainion.GraphViz.Modules.Metrics
{
    [ViewSortHint(Viewer.Abstractions.RegionNames.AddIns + ".2000")]
    internal partial class ToolsMenuItem : MenuItem
    {
        public ToolsMenuItem(ToolsMenuItemModel model)
        {
            InitializeComponent();

            DataContext = model;
        }
    }
}