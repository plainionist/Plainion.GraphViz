using System.ComponentModel.Composition.Hosting;
using System.IO;
using System.Windows;
using Plainion.GraphViz.Infrastructure.ViewModel;
using Prism.Interactivity;
using Prism.Regions;
using Plainion.Prism.Interactivity;
using Prism.Mef;
using System;

namespace Plainion.GraphViz.Viewer
{
    class Bootstrapper : MefBootstrapper
    {
        public bool Running { get; private set; }

        protected override DependencyObject CreateShell()
        {
            return Container.GetExportedValue<Shell>();
        }

        protected override void InitializeShell()
        {
            base.InitializeShell();

            App.Current.MainWindow = (Window)Shell;
            App.Current.MainWindow.Show();
        }

        protected override void ConfigureAggregateCatalog()
        {
            base.ConfigureAggregateCatalog();

            // Prism automatically loads the module with that line
            AggregateCatalog.Catalogs.Add(new AssemblyCatalog(GetType().Assembly));

            // add all the assemblies which are no modules
            AggregateCatalog.Catalogs.Add(new AssemblyCatalog(typeof(PopupWindowActionRegionAdapter).Assembly));
            AggregateCatalog.Catalogs.Add(new AssemblyCatalog(typeof(GraphView).Assembly));
            AggregateCatalog.Catalogs.Add(new AssemblyCatalog(typeof(IDomainModel).Assembly));

            // with ".Location" property we sometimes got strange error message that loading from
            // remote location is not allows
            var moduleRoot = Path.GetDirectoryName(new Uri(GetType().Assembly.CodeBase).LocalPath);
            try
            {
                foreach (var moduleFile in Directory.GetFiles(moduleRoot, "Plainion.GraphViz.Modules.*.dll"))
                {
                    AggregateCatalog.Catalogs.Add(new AssemblyCatalog(moduleFile));
                }
            }
            catch (Exception ex)
            {
                throw new Exception(string.Format("Loding from '{0}' failed", moduleRoot), ex);
            }
        }

        protected override RegionAdapterMappings ConfigureRegionAdapterMappings()
        {
            var mappings = base.ConfigureRegionAdapterMappings();
            mappings.RegisterMapping(typeof(PopupWindowAction), Container.GetExportedValue<PopupWindowActionRegionAdapter>());
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

        protected virtual void OnShutdown(object sender, ExitEventArgs e)
        {
            Container.Dispose();
        }
    }
}
