using System.ComponentModel.Composition;
using System.Windows.Controls;
using Plainion.GraphViz.Viewer.ViewModels;

namespace Plainion.GraphViz.Viewer.Views
{
    /// <summary>
    /// Interaction logic for SettingsWindow.xaml
    /// </summary>
    [Export(typeof(SettingsEditor))]
    public partial class SettingsEditor : UserControl
    {
        [ImportingConstructor]
        public SettingsEditor( SettingsEditorModel model )
        {
            InitializeComponent();

            DataContext = model;
        }
    }
}
