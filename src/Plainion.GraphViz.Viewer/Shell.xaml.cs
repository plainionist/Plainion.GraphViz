using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;

namespace Plainion.GraphViz.Viewer
{
    partial class Shell : Window
    {
        public Shell(ShellViewModel model)
        {
            InitializeComponent();

            DataContext = model;
        }

        private void OnTipsClick(object sender, RoutedEventArgs e)
        {
            var tooltip = ((ToolTip)((Hyperlink)sender).ToolTip);
            tooltip.IsOpen = !tooltip.IsOpen;
        }

        protected override void OnKeyDown(KeyEventArgs e)
        {
            if (e.Key == Key.F11)
            {
                if (WindowStyle == WindowStyle.None)
                {
                    WindowStyle = WindowStyle.SingleBorderWindow;
                    ResizeMode = ResizeMode.CanResize;
                    WindowState = WindowState.Normal;
                }
                else
                {
                    WindowStyle = WindowStyle.None;
                    ResizeMode = ResizeMode.NoResize;
                    WindowState = WindowState.Maximized;
                }

                e.Handled = true;
            }
            base.OnKeyDown(e);
        }
    }
}