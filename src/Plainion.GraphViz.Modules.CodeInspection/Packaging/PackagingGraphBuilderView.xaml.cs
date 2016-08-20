using System.ComponentModel.Composition;
using System.Windows.Controls;
using System.Linq;
using System.Windows;
using System.Windows.Documents;

namespace Plainion.GraphViz.Modules.CodeInspection.Packaging
{
    [Export( typeof( PackagingGraphBuilderView ) )]
    public partial class PackagingGraphBuilderView : UserControl
    {
        [ImportingConstructor]
        internal PackagingGraphBuilderView( PackagingGraphBuilderViewModel model )
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
