using System.ComponentModel.Composition;
using System.Windows.Controls;

namespace Plainion.GraphViz.Modules.Analysis
{
    [Export( typeof(Bookmarks) )]
    public partial class Bookmarks : UserControl
    {
        [ImportingConstructor]
        internal Bookmarks(BookmarksViewModel model )
        {
            InitializeComponent();

            DataContext = model;
        }
    }
}
