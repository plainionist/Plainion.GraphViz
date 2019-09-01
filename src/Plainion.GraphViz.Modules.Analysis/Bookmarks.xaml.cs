using System.Windows.Controls;

namespace Plainion.GraphViz.Modules.Analysis
{
    partial class Bookmarks : UserControl
    {
        public Bookmarks(BookmarksViewModel model )
        {
            InitializeComponent();

            DataContext = model;
        }
    }
}
