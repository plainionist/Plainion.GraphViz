using Plainion.GraphViz.Modules.CodeInspection.Packaging;
using Plainion.GraphViz.Modules.CodeInspection.Packaging.Actors;
using Prism.Ioc;
using Prism.Modularity;
using Prism.Navigation.Regions;

namespace Plainion.GraphViz.Modules.CodeInspection
{
    public class PackagingModule : IModule
    {
        private IRegionManager myRegionManager;

        public PackagingModule(IRegionManager regionManager)
        {
            myRegionManager = regionManager;
        }

        public void RegisterTypes(IContainerRegistry containerRegistry)
        {
            myRegionManager.RegisterViewWithRegion(Viewer.Abstractions.RegionNames.AddIns, typeof(PackagingGraphMenuItem));
            myRegionManager.RegisterViewWithRegion(RegionNames.PackagingGraphBuilder, typeof(PackagingGraphBuilderView));

            // explicitly register as singletons here to ensure that the host gets closed
            containerRegistry.RegisterSingleton<PackageAnalysisClient>();
        }

        public void OnInitialized(IContainerProvider containerProvider)
        {
        }
    }
}
