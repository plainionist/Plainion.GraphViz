using System.ComponentModel.Composition;
using System.Windows.Controls;

namespace Plainion.GraphViz.Modules.Analysis
{
    [Export( typeof( NodeMasksEditor ) )]
    public partial class NodeMasksEditor : UserControl
    {
        [ImportingConstructor]
        public NodeMasksEditor( NodeMasksEditorModel model )
        {
            InitializeComponent();

            DataContext = model;
        }
    }
}
