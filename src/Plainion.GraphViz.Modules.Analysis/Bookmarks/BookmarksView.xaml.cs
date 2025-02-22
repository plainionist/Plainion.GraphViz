using System.Windows.Controls;

namespace Plainion.GraphViz.Modules.Analysis.Bookmarks
{
    partial class BookmarksView : UserControl
    {
        public BookmarksView(BookmarksViewModel model )
        {
            InitializeComponent();

            DataContext = model;
        }
    }
}
