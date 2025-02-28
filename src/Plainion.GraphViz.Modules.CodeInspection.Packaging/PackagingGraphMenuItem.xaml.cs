using System.Windows.Controls;
using Prism.Navigation.Regions;

namespace Plainion.GraphViz.Modules.CodeInspection.Packaging
{
    [ViewSortHint(Viewer.Abstractions.RegionNames.AddIns + ".0030")]
    partial class PackagingGraphMenuItem : MenuItem
    {
        public PackagingGraphMenuItem( PackagingGraphMenuItemModel model )
        {
            InitializeComponent();

            DataContext = model;
        }
    }
}
