using System.ComponentModel.Composition;
using System.Windows.Controls;
using Plainion.GraphViz.Viewer.ViewModels;

namespace Plainion.GraphViz.Viewer.Views
{
    [Export( typeof( StatusMessagesView ) )]
    partial class StatusMessagesView : UserControl
    {
        [ImportingConstructor]
        public StatusMessagesView( StatusMessagesViewModel model )
        {
            InitializeComponent();

            DataContext = model;
        }
    }
}
