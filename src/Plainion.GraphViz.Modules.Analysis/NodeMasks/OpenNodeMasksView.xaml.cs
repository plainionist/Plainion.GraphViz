using System.Windows.Controls;

namespace Plainion.GraphViz.Modules.Analysis.NodeMasks;

partial class OpenNodeMasksView : UserControl
{
    public OpenNodeMasksView(OpenNodeMasksViewModel model)
    {
        InitializeComponent();

        DataContext = model;
    }
}
