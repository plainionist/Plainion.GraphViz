using System.Windows.Controls;

namespace Plainion.GraphViz.Modules.Properties;

partial class OpenPropertiesView : UserControl
{
    public OpenPropertiesView(OpenPropertiesViewModel model)
    {
        InitializeComponent();

        DataContext = model;
    }
}
