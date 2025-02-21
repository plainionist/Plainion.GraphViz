using Prism.Ioc;
using Prism.Modularity;
using Prism.Navigation.Regions;

namespace Plainion.GraphViz.Modules.Obfuscate;

public class ObfuscateModule : IModule
{
    private readonly IRegionManager myRegionManager;

    public ObfuscateModule(IRegionManager regionManager)
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
