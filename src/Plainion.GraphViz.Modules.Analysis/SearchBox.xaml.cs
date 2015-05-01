using System.ComponentModel.Composition;
using System.Windows.Controls;

namespace Plainion.GraphViz.Modules.Analysis
{
    [Export( typeof( SearchBox ) )]
    public partial class SearchBox : UserControl
    {
        [ImportingConstructor]
        internal SearchBox( SearchBoxModel model )
        {
            InitializeComponent();

            DataContext = model;
        }
    }
}
