using Prism.Ioc;
using Prism.Modularity;
using Prism.Navigation.Regions;

namespace Plainion.GraphViz.Modules.SDK
{
    // Hint: has to be unique across the whole application
    public class SdkModule : IModule
    {
        private readonly IRegionManager myRegionManager;

        public SdkModule(IRegionManager regionManager)
        {
            myRegionManager = regionManager;
        }

        public void RegisterTypes(IContainerRegistry containerRegistry)
        {
            myRegionManager.RegisterViewWithRegion(Viewer.Abstractions.RegionNames.AddIns, typeof(Analysis1.ToolsMenuItem));
            myRegionManager.RegisterViewWithRegion(RegionNames.SDKAnalysis1, typeof(Analysis1.ConfigurationView));

            // explicitly register as singletons here to ensure that the host gets closed
            containerRegistry.RegisterSingleton<Analysis1.Analyzer>();
        }

        public void OnInitialized(IContainerProvider containerProvider)
        {
        }
    }
}
