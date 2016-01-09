using System.ComponentModel;
using System.ComponentModel.Composition;
using System.Windows;
using System.Windows.Controls;

namespace Plainion.GraphViz.Modules.Editor
{
    [Export( typeof( DotLangEditorView ) )]
    public partial class DotLangEditorView : UserControl
    {
        [ImportingConstructor]
        internal DotLangEditorView( DotLangEditorViewModel model )
        {
            InitializeComponent();

            DataContext = model;
        }
    }
}
