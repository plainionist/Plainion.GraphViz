using System.Windows.Controls;

namespace Plainion.GraphViz.Modules.Metrics
{
    partial class MetricsView : UserControl
    {
        public MetricsView(MetricsViewModel model)
        {
            InitializeComponent();

            DataContext = model;
        }
    }
}
