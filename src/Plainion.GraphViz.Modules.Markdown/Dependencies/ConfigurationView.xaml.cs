using System.Windows.Controls;

namespace Plainion.GraphViz.Modules.Markdown.Dependencies
{
    internal partial class ConfigurationView : UserControl
    {
        public ConfigurationView(ConfigurationViewModel model)
        {
            InitializeComponent();

            DataContext = model;
        }
    }
}