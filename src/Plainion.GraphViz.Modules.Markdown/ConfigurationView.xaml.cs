using System.Windows.Controls;

namespace Plainion.GraphViz.Modules.Markdown
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