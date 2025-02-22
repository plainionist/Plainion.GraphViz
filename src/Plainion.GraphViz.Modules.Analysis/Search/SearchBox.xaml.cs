using System.Windows.Controls;

namespace Plainion.GraphViz.Modules.Analysis.Search
{
    partial class SearchBox : UserControl
    {
        public SearchBox( SearchBoxModel model )
        {
            InitializeComponent();

            DataContext = model;
        }
    }
}
