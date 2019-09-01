using System.Windows.Controls;

namespace Plainion.GraphViz.Modules.Documents
{
    partial class OpenDocumentsView : UserControl
    {
        public OpenDocumentsView(OpenDocumentsViewModel model)
        {
            InitializeComponent();

            DataContext = model;
        }
    }
}
