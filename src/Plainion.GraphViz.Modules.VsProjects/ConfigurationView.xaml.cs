using System.Windows.Controls;

namespace Plainion.GraphViz.Modules.VsProjects
{
    partial class ConfigurationView : UserControl
    {
        public ConfigurationView(ConfigurationViewModel model)
        {
            InitializeComponent();

            DataContext = model;
        }
    }
}
