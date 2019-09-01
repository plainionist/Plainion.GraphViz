using System.Windows.Controls;

namespace Plainion.GraphViz.Modules.Analysis
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
