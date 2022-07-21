using System.Windows.Controls;

namespace Plainion.GraphViz.Modules.SDK.Analysis1
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
