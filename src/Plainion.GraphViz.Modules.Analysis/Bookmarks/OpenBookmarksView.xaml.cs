using System.Windows.Controls;

namespace Plainion.GraphViz.Modules.Analysis.Bookmarks;

partial class OpenBookmarksView : UserControl
{
    public OpenBookmarksView(OpenBookmarksViewModel model)
    {
        InitializeComponent();

        DataContext = model;
    }
}
