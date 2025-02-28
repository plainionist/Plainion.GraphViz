using System.Windows.Controls;
using Prism.Navigation.Regions;

namespace Plainion.GraphViz.Modules.CodeInspection.Packaging
{
    [ViewSortHint("tool-0030")]
    partial class PackagingGraphMenuItem : MenuItem
    {
        public PackagingGraphMenuItem( PackagingGraphMenuItemModel model )
        {
            InitializeComponent();

            DataContext = model;
        }
    }
}
