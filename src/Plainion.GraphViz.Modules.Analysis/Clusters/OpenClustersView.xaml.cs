using System.Windows.Controls;

namespace Plainion.GraphViz.Modules.Analysis.Clusters;

partial class OpenClustersView : UserControl
{
    public OpenClustersView(OpenClustersViewModel model)
    {
        InitializeComponent();

        DataContext = model;
    }
}
