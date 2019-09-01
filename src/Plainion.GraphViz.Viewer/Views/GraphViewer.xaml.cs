using System.Windows.Controls;
using Plainion.GraphViz.Viewer.ViewModels;

namespace Plainion.GraphViz.Viewer.Views
{
    partial class GraphViewer : UserControl
    {
        public GraphViewer( GraphViewerModel model )
        {
            InitializeComponent();

            DataContext = model;
        }

        public GraphView GraphView { get { return myGraphView; } }
    }
}
