using System.ComponentModel.Composition;
using System.Windows.Controls;

namespace Plainion.GraphViz.Modules.Analysis
{
    [Export( typeof( SearchBox ) )]
    partial class SearchBox : UserControl
    {
        [ImportingConstructor]
        public SearchBox( SearchBoxModel model )
        {
            InitializeComponent();

            DataContext = model;
        }
    }
}
