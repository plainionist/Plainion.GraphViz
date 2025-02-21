using System.Linq;
using System.Windows.Controls;

namespace Plainion.GraphViz.Modules.CodeInspection.Packaging
{
    partial class PackagingGraphBuilderView : UserControl
    {
        public PackagingGraphBuilderView( PackagingGraphBuilderViewModel model )
        {
            InitializeComponent();

            DataContext = model;

            myPackagesFilter.SelectionChanged += myPackagesFilter_SelectionChanged;
        }

        private void myPackagesFilter_SelectionChanged( object sender, SelectionChangedEventArgs e )
        {
            ( ( PackagingGraphBuilderViewModel )DataContext ).PackagesToAnalyze = myPackagesFilter.SelectedItems
                .OfType<string>()
                .ToList();
        }
    }
}
