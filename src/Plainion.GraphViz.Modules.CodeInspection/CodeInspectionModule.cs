﻿using System.ComponentModel.Composition;
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
        private IRegionManager myRegionManager;

        [ImportingConstructor]
        public CodeInspectionModule(IRegionManager regionManager)
        {
            myRegionManager = regionManager;
        }

        public void Initialize()
        {
            myRegionManager.RegisterViewWithRegion(Infrastructure.RegionNames.AddIns, typeof(InheritanceGraphMenuItem));
            myRegionManager.RegisterViewWithRegion(RegionNames.InheritanceGraphBuilder, typeof(InheritanceGraphBuilderView));

            myRegionManager.RegisterViewWithRegion(Infrastructure.RegionNames.AddIns, typeof(PackagingGraphMenuItem));
            myRegionManager.RegisterViewWithRegion(RegionNames.PackagingGraphBuilder, typeof(PackagingGraphBuilderView));

            myRegionManager.RegisterViewWithRegion(Infrastructure.RegionNames.AddIns, typeof(PathFinderMenuItem));
            myRegionManager.RegisterViewWithRegion(RegionNames.PathFinder, typeof(PathFinderView));

            myRegionManager.RegisterViewWithRegion(Infrastructure.RegionNames.AddIns, typeof(CallTreeMenuItem));
            myRegionManager.RegisterViewWithRegion(RegionNames.CallTree, typeof(CallTreeView));
        }
    }
}
