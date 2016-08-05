using System.Windows;
using System.Windows.Threading;

namespace Plainion.GraphViz.Viewer
{
    public partial class App : Application
    {
        private void OnUnhandledException( object sender, DispatcherUnhandledExceptionEventArgs e )
        {
            MessageBox.Show( e.Exception.Dump(), "Unhandled exception", MessageBoxButton.OK, MessageBoxImage.Error );
            e.Handled = true;
        }

        protected override void OnStartup( StartupEventArgs e )
        {
            ShutdownMode = ShutdownMode.OnMainWindowClose;

            base.OnStartup( e );

            new Bootstrapper().Run();
        }
    }
}