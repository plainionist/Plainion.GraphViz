using System.Windows.Controls;

namespace Plainion.GraphViz.Modules.Markdown
{
    internal partial class ToolsMenuItem : MenuItem
    {
        public ToolsMenuItem(ToolsMenuItemModel model)
        {
            InitializeComponent();

            DataContext = model;
        }
    }
}