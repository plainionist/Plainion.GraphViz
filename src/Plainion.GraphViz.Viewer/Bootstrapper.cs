using System.ComponentModel.Composition.Hosting;
using System.IO;
using System.Windows;
using Plainion.GraphViz.Infrastructure.ViewModel;
using Microsoft.Practices.Prism.Interactivity;
using Microsoft.Practices.Prism.MefExtensions;
using Microsoft.Practices.Prism.Regions;
using Plainion.Prism.Interactivity;

namespace Plainion.GraphViz.Viewer
{
    public class Bootstrapper : MefBootstrapper
    {
        protected override DependencyObject CreateShell()
        {
            return Container.GetExportedValue<Shell>();
        }

        protected override void InitializeShell()
        {
            base.InitializeShell();

            App.Current.MainWindow = ( Window )Shell;
            App.Current.MainWindow.Show();
        }

        protected override void ConfigureAggregateCatalog()
        {
            base.ConfigureAggregateCatalog();

            // Prism automatically loads the module with that line
            AggregateCatalog.Catalogs.Add( new AssemblyCatalog( GetType().Assembly ) );

            // add all the assemblies which are no modules
            AggregateCatalog.Catalogs.Add( new AssemblyCatalog( typeof( PopupWindowActionRegionAdapter ).Assembly ) );
            AggregateCatalog.Catalogs.Add( new AssemblyCatalog( typeof( GraphView ).Assembly ) );
            AggregateCatalog.Catalogs.Add( new AssemblyCatalog( typeof( IDomainModel ).Assembly ) );

            var moduleRoot = Path.GetDirectoryName( GetType().Assembly.Location );
            foreach( var moduleFile in Directory.GetFiles( moduleRoot, "Plainion.GraphViz.Modules.*.dll" ) )
            {
                AggregateCatalog.Catalogs.Add( new AssemblyCatalog( moduleFile ) );
            }
        }

        protected override Microsoft.Practices.Prism.Regions.RegionAdapterMappings ConfigureRegionAdapterMappings()
        {
            var mappings = base.ConfigureRegionAdapterMappings();
            mappings.RegisterMapping( typeof( PopupWindowAction ), Container.GetExportedValue<PopupWindowActionRegionAdapter>() );
            return mappings;
        }

        public override void Run( bool runWithDefaultConfiguration )
        {
            var helpRoot = Path.Combine( Path.GetDirectoryName( GetType().Assembly.Location ), "Help" );
            Help.Server.Start( helpRoot )
                .ContinueWith( t => HelpClient.Port = t.Result );

            base.Run( runWithDefaultConfiguration );

            Application.Current.Exit += OnShutdown;

            // we have to call this here in order to support regions which are provided by modules
            RegionManager.UpdateRegions();
        }

        protected virtual void OnShutdown( object sender, ExitEventArgs e )
        {
            Help.Server.Stop();
            Container.Dispose();
        }
    }
}
