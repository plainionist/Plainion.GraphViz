using System;
using System.IO;
using System.Windows;
using System.Windows.Threading;
using Plainion;
using Plainion.GraphViz.Infrastructure.Services;
using Plainion.GraphViz.Infrastructure.ViewModel;
using Plainion.GraphViz.Viewer;
using Plainion.GraphViz.Viewer.Services;
using Plainion.Prism.Interactivity;
using Prism.Ioc;
using Prism.Modularity;
using Prism.Regions;
using Prism.Regions.Behaviors;
using Prism.Unity;
using Unity;

namespace PlainionGraphViz.Viewer
{
    public partial class App : PrismApplication
    {
        private bool myRunning;

        protected override Window CreateShell()
        {
            // workaround to get all module classes added to container before shell is resolved
            // this is needed e.g. for "IDocumentLoader"
            InitializeModules();

            return Container.Resolve<Shell>();
        }

        protected override IModuleCatalog CreateModuleCatalog()
        {
            var catalog = new DirectoryModuleCatalog();

            // explicitly add core module which is not found by the DirectoryModuleCatalog as it only searches for ".dll"
            catalog.AddModule(new ModuleInfo(typeof(CoreModule).FullName, typeof(CoreModule).AssemblyQualifiedName));

            // with ".Location" property we sometimes got strange error message that loading from
            // remote location is not allows
            catalog.ModulePath = Path.GetDirectoryName(GetType().Assembly.Location);

            return catalog;
        }

        protected override void RegisterTypes(IContainerRegistry containerRegistry)
        {
            containerRegistry.RegisterSingleton<IStatusMessageService, StatusMessageService>();
            containerRegistry.RegisterSingleton<IPresentationCreationService, PresentationCreationService>();
            containerRegistry.RegisterSingleton<ConfigurationService>();
            containerRegistry.Register<DelayedRegionCreationBehavior, KeepAliveDelayedRegionCreationBehavior>();

            containerRegistry.RegisterSingleton<IDomainModel, DefaultDomainModel>();
        }

        protected override void ConfigureRegionAdapterMappings(RegionAdapterMappings regionAdapterMappings)
        {
            base.ConfigureRegionAdapterMappings(regionAdapterMappings);

            regionAdapterMappings.RegisterMapping(typeof(PopupWindowAction), Container.Resolve<PopupWindowActionRegionAdapter>());
        }

        protected override void OnInitialized()
        {
            // we have to call this here in order to support regions which are provided by modules
            RegionManager.UpdateRegions();

            myRunning = true;
        }

        private void OnUnhandledException(object sender, DispatcherUnhandledExceptionEventArgs e)
        {
            MessageBox.Show(e.Exception.Dump(), "Unhandled exception", MessageBoxButton.OK, MessageBoxImage.Error);
            e.Handled = true;

            if (!myRunning)
            {
                // Ensure process gets really closed in case of exception during startup
                // We do not set "e.Handled" to false to avoid "stopped working" dialog
                Environment.Exit(1);
            }
        }

        protected override void OnStartup(StartupEventArgs e)
        {
            ShutdownMode = ShutdownMode.OnMainWindowClose;

            base.OnStartup(e);
        }
    }
}