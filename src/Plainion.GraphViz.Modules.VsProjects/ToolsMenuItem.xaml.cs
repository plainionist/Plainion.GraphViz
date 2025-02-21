using System.Windows.Controls;

namespace Plainion.GraphViz.Modules.VsProjects
{
    partial class ToolsMenuItem : MenuItem
    {
        public ToolsMenuItem(ToolsMenuItemModel model)
        {
            InitializeComponent();

            DataContext = model;
        }
    }
}
