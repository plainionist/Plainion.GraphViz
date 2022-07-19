using System.Windows.Controls;

namespace Plainion.GraphViz.Modules.VsProjects.Dependencies
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
