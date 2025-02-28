using System.Windows.Controls;

namespace Plainion.GraphViz.Modules.Properties
{
    partial class PropertiesView : UserControl
    {
        public PropertiesView(PropertiesViewModel model)
        {
            InitializeComponent();

            DataContext = model;
        }
    }
}
