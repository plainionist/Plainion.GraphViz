using System.Windows.Controls;

namespace Plainion.GraphViz.Modules.SDK.Analysis1
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
