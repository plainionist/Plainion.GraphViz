using System.Windows.Controls;
using Prism.Navigation.Regions;

namespace Plainion.GraphViz.Modules.Analysis.NodeMasks;

[ViewSortHint(Viewer.Abstractions.RegionNames.SecondaryToolBox + ".0020")]
partial class OpenNodeMasksView : UserControl
{
    public OpenNodeMasksView(OpenNodeMasksViewModel model)
    {
        InitializeComponent();

        DataContext = model;
    }
}
