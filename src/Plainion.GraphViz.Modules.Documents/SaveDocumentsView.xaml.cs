using System.Windows.Controls;

namespace Plainion.GraphViz.Modules.Documents
{
    partial class SaveDocumentsView : UserControl
    {
        public SaveDocumentsView(SaveDocumentsViewModel model)
        {
            InitializeComponent();

            DataContext = model;
        }
    }
}
