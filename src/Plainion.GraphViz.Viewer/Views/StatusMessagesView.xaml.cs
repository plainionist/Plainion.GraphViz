using System.ComponentModel.Composition;
using System.Windows.Controls;
using Plainion.GraphViz.Viewer.ViewModels;

namespace Plainion.GraphViz.Viewer.Views
{
    [Export( typeof( StatusMessagesView ) )]
    public partial class StatusMessagesView : UserControl
    {
        [ImportingConstructor]
        internal StatusMessagesView( StatusMessagesViewModel model )
        {
            InitializeComponent();

            DataContext = model;
        }
    }
}
