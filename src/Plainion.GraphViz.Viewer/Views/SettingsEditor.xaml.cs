using System.ComponentModel.Composition;
using System.Windows.Controls;
using Plainion.GraphViz.Viewer.ViewModels;

namespace Plainion.GraphViz.Viewer.Views
{
    [Export(typeof(SettingsEditor))]
    partial class SettingsEditor : UserControl
    {
        [ImportingConstructor]
        public SettingsEditor( SettingsEditorModel model )
        {
            InitializeComponent();

            DataContext = model;
        }
    }
}
