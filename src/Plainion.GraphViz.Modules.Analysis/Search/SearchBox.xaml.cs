using System.Windows.Controls;
using Prism.Navigation.Regions;

namespace Plainion.GraphViz.Modules.Analysis.Search
{
    [ViewSortHint(Viewer.Abstractions.RegionNames.SecondaryToolBox + ".0010")]
    partial class SearchBox : UserControl
    {
        public SearchBox( SearchBoxModel model )
        {
            InitializeComponent();

            DataContext = model;
        }
    }
}
