using System;
using System.IO;
using System.Windows;
using Plainion.GraphViz.Infrastructure.Services;
using Plainion.GraphViz.Infrastructure.ViewModel;
using Plainion.GraphViz.Viewer.Services;
using Plainion.Prism.Interactivity;
using Prism.Interactivity;
using Prism.Modularity;
using Prism.Regions;
using Prism.Regions.Behaviors;
using Prism.Unity;
using Unity;
using Unity.Lifetime;

namespace Plainion.GraphViz.Viewer
{
    class Bootstrapper : UnityBootstrapper
    {
        public bool Running { get; private set; }

        protected override DependencyObject CreateShell()
        {
            // workaround to get all module classes added to container before shell is resolved
            // this is needed e.g. for "IDocumentLoader"
            InitializeModules();

            return Container.Resolve<Shell>();
        }

        protected override void InitializeShell()
        {
            base.InitializeShell();

            App.Current.MainWindow = (Window)Shell;
            App.Current.MainWindow.Show();
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

        protected override void ConfigureContainer()
        {
            base.ConfigureContainer();

            Container.RegisterType<IStatusMessageService, StatusMessageService>(new ContainerControlledLifetimeManager());
            Container.RegisterType<IPresentationCreationService, PresentationCreationService>(new ContainerControlledLifetimeManager());
            Container.RegisterType<ConfigurationService>(new ContainerControlledLifetimeManager());
            Container.RegisterType<DelayedRegionCreationBehavior, KeepAliveDelayedRegionCreationBehavior>(new TransientLifetimeManager());

            Container.RegisterType<IDomainModel, DefaultDomainModel>(new ContainerControlledLifetimeManager());
        }

        protected override RegionAdapterMappings ConfigureRegionAdapterMappings()
        {
            var mappings = base.ConfigureRegionAdapterMappings();

            mappings.RegisterMapping(typeof(PopupWindowAction), Container.Resolve<PopupWindowActionRegionAdapter>());

            return mappings;
        }

        public override void Run(bool runWithDefaultConfiguration)
        {
            base.Run(runWithDefaultConfiguration);

            Application.Current.Exit += OnShutdown;

            // we have to call this here in order to support regions which are provided by modules
            RegionManager.UpdateRegions();

            Running = true;
        }

        private void OnShutdown(object sender, ExitEventArgs e)
        {
            Container.Dispose();
        }
    }
}
