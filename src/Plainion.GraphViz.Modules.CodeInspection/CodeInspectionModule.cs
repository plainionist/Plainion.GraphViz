using Plainion.GraphViz.Modules.CodeInspection.CallTree;
using Plainion.GraphViz.Modules.CodeInspection.CallTree.Actors;
using Plainion.GraphViz.Modules.CodeInspection.Inheritance;
using Plainion.GraphViz.Modules.CodeInspection.Inheritance.Actors;
using Plainion.GraphViz.Modules.CodeInspection.Packaging;
using Plainion.GraphViz.Modules.CodeInspection.Packaging.Actors;
using Plainion.GraphViz.Modules.CodeInspection.PathFinder;
using Plainion.GraphViz.Modules.CodeInspection.PathFinder.Actors;
using Prism.Ioc;
using Prism.Modularity;
using Prism.Regions;

namespace Plainion.GraphViz.Modules.CodeInspection
{
    public class CodeInspectionModule : IModule
    {
        private IRegionManager myRegionManager;

        public CodeInspectionModule(IRegionManager regionManager)
        {
            myRegionManager = regionManager;
        }

        public void RegisterTypes(IContainerRegistry containerRegistry)
        {
            myRegionManager.RegisterViewWithRegion(Infrastructure.RegionNames.AddIns, typeof(InheritanceGraphMenuItem));
            myRegionManager.RegisterViewWithRegion(RegionNames.InheritanceGraphBuilder, typeof(InheritanceGraphBuilderView));

            myRegionManager.RegisterViewWithRegion(Infrastructure.RegionNames.AddIns, typeof(PackagingGraphMenuItem));
            myRegionManager.RegisterViewWithRegion(RegionNames.PackagingGraphBuilder, typeof(PackagingGraphBuilderView));

            myRegionManager.RegisterViewWithRegion(Infrastructure.RegionNames.AddIns, typeof(PathFinderMenuItem));
            myRegionManager.RegisterViewWithRegion(RegionNames.PathFinder, typeof(PathFinderView));

            myRegionManager.RegisterViewWithRegion(Infrastructure.RegionNames.AddIns, typeof(CallTreeMenuItem));
            myRegionManager.RegisterViewWithRegion(RegionNames.CallTree, typeof(CallTreeView));

            // explicitly register as singletons here to ensure that the host gets closed
            containerRegistry.RegisterSingleton<PackageAnalysisClient>();
            containerRegistry.RegisterSingleton<InheritanceClient>();
            containerRegistry.RegisterSingleton<CallTreeClient>();
            containerRegistry.RegisterSingleton<PathFinderClient>();
        }

        public void OnInitialized(IContainerProvider containerProvider)
        {
        }
    }
}
