using System.Windows.Controls;
using Prism.Navigation.Regions;

namespace Plainion.GraphViz.Modules.Documents
{
    [ViewSortHint(Viewer.Abstractions.RegionNames.PrimaryToolBox + ".0020")]
    partial class SaveDocumentsView : UserControl
    {
        public SaveDocumentsView(SaveDocumentsViewModel model)
        {
            InitializeComponent();

            DataContext = model;
        }
    }
}
