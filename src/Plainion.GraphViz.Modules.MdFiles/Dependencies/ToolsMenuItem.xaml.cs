using System.Windows.Controls;

namespace Plainion.GraphViz.Modules.MdFiles.Dependencies
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