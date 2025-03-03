using Prism.Ioc;
using Prism.Modularity;
using Prism.Navigation.Regions;

namespace Plainion.GraphViz.Modules.Metrics;

public class MetricsModule(IRegionManager regionManager) : IModule
{
    public void RegisterTypes(IContainerRegistry containerRegistry)
    {
        regionManager.RegisterViewWithRegion(Viewer.Abstractions.RegionNames.AddIns, typeof(ToolsMenuItem));
        regionManager.RegisterViewWithRegion(RegionNames.Metrics, typeof(MetricsView));
    }

    public void OnInitialized(IContainerProvider containerProvider) { }
}
