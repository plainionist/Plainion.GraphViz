using System.ComponentModel.Composition;
using System.Windows.Controls;

namespace Plainion.GraphViz.Modules.Analysis
{
    [Export( typeof(Bookmarks) )]
    partial class Bookmarks : UserControl
    {
        [ImportingConstructor]
        public Bookmarks(BookmarksViewModel model )
        {
            InitializeComponent();

            DataContext = model;
        }
    }
}
