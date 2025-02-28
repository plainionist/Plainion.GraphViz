using System.Windows.Controls;
using Prism.Navigation.Regions;

namespace Plainion.GraphViz.Modules.Analysis.Clusters;

[ViewSortHint(Viewer.Abstractions.RegionNames.SecondaryToolBox + ".0030")]
partial class OpenClustersView : UserControl
{
    public OpenClustersView(OpenClustersViewModel model)
    {
        InitializeComponent();

        DataContext = model;
    }
}
