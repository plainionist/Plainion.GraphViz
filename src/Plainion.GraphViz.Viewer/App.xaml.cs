using System;
using System.Windows;
using System.Windows.Threading;

namespace Plainion.GraphViz.Viewer
{
    public partial class App : Application
    {
        private Bootstrapper myBootstrapper = new Bootstrapper();

        private void OnUnhandledException( object sender, DispatcherUnhandledExceptionEventArgs e )
        {
            MessageBox.Show( e.Exception.Dump(), "Unhandled exception", MessageBoxButton.OK, MessageBoxImage.Error );
            e.Handled = true;

            if (!myBootstrapper.Running)
            {
                // Ensure process gets really closed in case of exception during startup
                // We do not set "e.Handled" to false to avoid "stopped working" dialog
                Environment.Exit(1);
            }
        }

        protected override void OnStartup( StartupEventArgs e )
        {
            ShutdownMode = ShutdownMode.OnMainWindowClose;

            base.OnStartup( e );

            myBootstrapper.Run();
        }
    }
}