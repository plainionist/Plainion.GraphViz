using System.ComponentModel.Composition;
using System.Windows.Controls;
using Plainion.GraphViz.Viewer.ViewModels;

namespace Plainion.GraphViz.Viewer.Views
{
    [Export( typeof( GraphViewer ) )]
    public partial class GraphViewer : UserControl
    {
        [ImportingConstructor]
        internal GraphViewer( GraphViewerModel model )
        {
            InitializeComponent();

            DataContext = model;
        }

        public GraphView GraphView { get { return myGraphView; } }
    }
}
