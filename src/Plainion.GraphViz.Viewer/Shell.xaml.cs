using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;

namespace Plainion.GraphViz.Viewer
{
    partial class Shell : Window
    {
        public Shell( ShellViewModel model )
        {
            InitializeComponent();

            DataContext = model;
        }

        private void OnTipsClick( object sender, RoutedEventArgs e )
        {
            var tooltip = ( ( ToolTip )( ( Hyperlink )sender ).ToolTip );
            tooltip.IsOpen = !tooltip.IsOpen;
        }
    }
}