using System.Windows.Controls;

namespace Plainion.GraphViz.Modules.CodeInspection.Packaging
{
    partial class PackagingGraphMenuItem : MenuItem
    {
        public PackagingGraphMenuItem( PackagingGraphMenuItemModel model )
        {
            InitializeComponent();

            DataContext = model;
        }
    }
}
