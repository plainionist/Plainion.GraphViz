using Prism.Ioc;
using Prism.Modularity;
using Prism.Navigation.Regions;

namespace Plainion.GraphViz.Modules.Properties;

public class PropertiesModule : IModule
{
    private readonly IRegionManager myRegionManager;

    public PropertiesModule(IRegionManager regionManager)
    {
        myRegionManager = regionManager;
    }

    public void RegisterTypes(IContainerRegistry containerRegistry)
    {
        myRegionManager.RegisterViewWithRegion(Viewer.Abstractions.RegionNames.AddIns, typeof(ToolsMenuItem));
    }

    public void OnInitialized(IContainerProvider containerProvider)
    {
    }
}
