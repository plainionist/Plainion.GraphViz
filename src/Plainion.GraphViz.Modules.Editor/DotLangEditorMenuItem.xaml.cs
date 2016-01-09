using System.ComponentModel.Composition;
using System.Windows.Controls;

namespace Plainion.GraphViz.Modules.Editor
{
    [Export( typeof( DotLangEditorMenuItem ) )]
    public partial class DotLangEditorMenuItem : MenuItem
    {
        [ImportingConstructor]
        public DotLangEditorMenuItem( DotLangEditorMenuItemModel model )
        {
            InitializeComponent();

            DataContext = model;
        }
    }
}
