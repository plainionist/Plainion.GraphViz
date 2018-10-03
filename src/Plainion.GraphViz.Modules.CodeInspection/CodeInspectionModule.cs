using System.ComponentModel.Composition;
using Plainion.GraphViz.Modules.CodeInspection.CallTree;
using Plainion.GraphViz.Modules.CodeInspection.Inheritance;
using Plainion.GraphViz.Modules.CodeInspection.Packaging;
using Plainion.GraphViz.Modules.CodeInspection.PathFinder;
using Prism.Mef.Modularity;
using Prism.Modularity;
using Prism.Regions;

namespace Plainion.GraphViz.Modules.CodeInspection
{
    [ModuleExport(typeof(CodeInspectionModule))]
    public class CodeInspectionModule : IModule
    {
        [Import]
        public IRegionManager RegionManager { get; set; }

        public void Initialize()
        {
            RegionManager.RegisterViewWithRegion(Infrastructure.RegionNames.AddIns, typeof(InheritanceGraphMenuItem));
            RegionManager.RegisterViewWithRegion(RegionNames.InheritanceGraphBuilder, typeof(InheritanceGraphBuilderView));

            RegionManager.RegisterViewWithRegion(Infrastructure.RegionNames.AddIns, typeof(PackagingGraphMenuItem));
            RegionManager.RegisterViewWithRegion(RegionNames.PackagingGraphBuilder, typeof(PackagingGraphBuilderView));

            RegionManager.RegisterViewWithRegion(Infrastructure.RegionNames.AddIns, typeof(PathFinderMenuItem));
            RegionManager.RegisterViewWithRegion(RegionNames.PathFinder, typeof(PathFinderView));

            RegionManager.RegisterViewWithRegion(Infrastructure.RegionNames.AddIns, typeof(CallTreeMenuItem));
            RegionManager.RegisterViewWithRegion(RegionNames.CallTree, typeof(CallTreeView));
        }
    }
}
