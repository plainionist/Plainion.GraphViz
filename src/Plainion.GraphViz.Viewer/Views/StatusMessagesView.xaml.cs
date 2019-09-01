using System.Windows.Controls;
using Plainion.GraphViz.Viewer.ViewModels;

namespace Plainion.GraphViz.Viewer.Views
{
    partial class StatusMessagesView : UserControl
    {
        public StatusMessagesView( StatusMessagesViewModel model )
        {
            InitializeComponent();

            DataContext = model;
        }
    }
}
