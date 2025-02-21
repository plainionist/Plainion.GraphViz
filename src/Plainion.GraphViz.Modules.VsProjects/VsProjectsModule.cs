using Prism.Ioc;
using Prism.Modularity;
using Prism.Navigation.Regions;

namespace Plainion.GraphViz.Modules.VsProjects
{
    public class VsProjectsModule : IModule
    {
        private readonly IRegionManager myRegionManager;

        public VsProjectsModule(IRegionManager regionManager)
        {
            myRegionManager = regionManager;
        }

        public void RegisterTypes(IContainerRegistry containerRegistry)
        {
            myRegionManager.RegisterViewWithRegion(Viewer.Abstractions.RegionNames.AddIns, typeof(ToolsMenuItem));
            myRegionManager.RegisterViewWithRegion(RegionNames.VsProjectDependencies, typeof(ConfigurationView));

            // explicitly register as singletons here to ensure that the host gets closed
            containerRegistry.RegisterSingleton<Analyzer>();
        }

        public void OnInitialized(IContainerProvider containerProvider)
        {
        }
    }
}
