using System.Windows.Controls;
using Plainion.GraphViz.Viewer.ViewModels;

namespace Plainion.GraphViz.Viewer.Views
{
    partial class SettingsEditor : UserControl
    {
        public SettingsEditor( SettingsEditorModel model )
        {
            InitializeComponent();

            DataContext = model;
        }
    }
}
