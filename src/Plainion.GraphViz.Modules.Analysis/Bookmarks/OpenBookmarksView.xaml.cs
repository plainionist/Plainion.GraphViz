using System.Windows.Controls;
using Prism.Navigation.Regions;

namespace Plainion.GraphViz.Modules.Analysis.Bookmarks;

[ViewSortHint(Viewer.Abstractions.RegionNames.SecondaryToolBox + ".0040")]
partial class OpenBookmarksView : UserControl
{
    public OpenBookmarksView(OpenBookmarksViewModel model)
    {
        InitializeComponent();

        DataContext = model;
    }
}
