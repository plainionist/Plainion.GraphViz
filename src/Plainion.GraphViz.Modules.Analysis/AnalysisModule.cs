using System.ComponentModel.Composition;
using Prism.Mef.Modularity;
using Prism.Modularity;
using Prism.Regions;

namespace Plainion.GraphViz.Modules.Analysis
{
    [ModuleExport(typeof(AnalysisModule))]
    public class AnalysisModule : IModule
    {
        private IRegionManager myRegionManager;

        [ImportingConstructor]
        public AnalysisModule(IRegionManager regionManager)
        {
            myRegionManager = regionManager;
        }

        public void Initialize()
        {
            myRegionManager.RegisterViewWithRegion(GraphViz.Infrastructure.RegionNames.SearchBox, typeof(SearchBox));
            myRegionManager.RegisterViewWithRegion(GraphViz.Infrastructure.RegionNames.NodeMasksEditor, typeof(NodeMasksEditor));
            myRegionManager.RegisterViewWithRegion(GraphViz.Infrastructure.RegionNames.NodeMasksView, typeof(NodeMasksView));
            myRegionManager.RegisterViewWithRegion(GraphViz.Infrastructure.RegionNames.ClusterEditor, typeof(ClusterEditor));
            myRegionManager.RegisterViewWithRegion(GraphViz.Infrastructure.RegionNames.Bookmarks, typeof(Bookmarks));
        }
    }
}
