using Prism.Ioc;
using Prism.Modularity;
using Prism.Navigation.Regions;

namespace Plainion.GraphViz.Modules.Properties;

public class PropertiesModule(IRegionManager regionManager) : IModule
{
    public void RegisterTypes(IContainerRegistry containerRegistry)
    {
        regionManager.RegisterViewWithRegion(Viewer.Abstractions.RegionNames.ToolBox_Properties, typeof(OpenPropertiesView));
        regionManager.RegisterViewWithRegion(RegionNames.Properties, typeof(PropertiesView));
    }

    public void OnInitialized(IContainerProvider containerProvider) { }
}
