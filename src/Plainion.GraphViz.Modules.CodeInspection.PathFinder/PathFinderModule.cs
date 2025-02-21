using Plainion.GraphViz.Modules.CodeInspection.PathFinder;
using Plainion.GraphViz.Modules.CodeInspection.PathFinder.Actors;
using Prism.Ioc;
using Prism.Modularity;
using Prism.Navigation.Regions;

namespace Plainion.GraphViz.Modules.CodeInspection
{
    public class PathFinderModule : IModule
    {
        private IRegionManager myRegionManager;

        public PathFinderModule(IRegionManager regionManager)
        {
            myRegionManager = regionManager;
        }

        public void RegisterTypes(IContainerRegistry containerRegistry)
        {
            myRegionManager.RegisterViewWithRegion(Viewer.Abstractions.RegionNames.AddIns, typeof(PathFinderMenuItem));
            myRegionManager.RegisterViewWithRegion(RegionNames.PathFinder, typeof(PathFinderView));

            // explicitly register as singletons here to ensure that the host gets closed
            containerRegistry.RegisterSingleton<PathFinderClient>();
        }

        public void OnInitialized(IContainerProvider containerProvider)
        {
        }
    }
}
