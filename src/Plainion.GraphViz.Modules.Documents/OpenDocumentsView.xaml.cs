using System.Windows.Controls;
using Prism.Navigation.Regions;

namespace Plainion.GraphViz.Modules.Documents
{
    [ViewSortHint(Viewer.Abstractions.RegionNames.PrimaryToolBox + ".0010")]
    partial class OpenDocumentsView : UserControl
    {
        public OpenDocumentsView(OpenDocumentsViewModel model)
        {
            InitializeComponent();

            DataContext = model;
        }
    }
}
