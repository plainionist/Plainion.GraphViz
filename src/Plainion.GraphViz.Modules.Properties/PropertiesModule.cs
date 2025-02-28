using Prism.Ioc;
using Prism.Modularity;
using Prism.Navigation.Regions;

namespace Plainion.GraphViz.Modules.Properties;

public class PropertiesModule(IRegionManager regionManager) : IModule
{
    public void RegisterTypes(IContainerRegistry containerRegistry)
    {
        regionManager.RegisterViewWithRegion(Viewer.Abstractions.RegionNames.GraphProperties, typeof(OpenPropertiesView));
    }

    public void OnInitialized(IContainerProvider containerProvider) { }
}
