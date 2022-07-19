using System.Windows.Controls;

namespace Plainion.GraphViz.Modules.VsProjects.Dependencies
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
