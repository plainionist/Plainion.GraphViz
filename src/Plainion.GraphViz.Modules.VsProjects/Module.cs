using Prism.Ioc;
using Prism.Modularity;
using Prism.Regions;

namespace Plainion.GraphViz.Modules.VsProjects
{
    public class Module : IModule
    {
        private readonly IRegionManager myRegionManager;

        public Module(IRegionManager regionManager)
        {
            myRegionManager = regionManager;
        }

        public void RegisterTypes(IContainerRegistry containerRegistry)
        {
            myRegionManager.RegisterViewWithRegion(Infrastructure.RegionNames.AddIns, typeof(Dependencies.ToolsMenuItem));
            myRegionManager.RegisterViewWithRegion(RegionNames.VsProjectDependencies, typeof(Dependencies.ConfigurationView));

            // explicitly register as singletons here to ensure that the host gets closed
            containerRegistry.RegisterSingleton<Dependencies.Analyzer>();
        }

        public void OnInitialized(IContainerProvider containerProvider)
        {
        }
    }
}
