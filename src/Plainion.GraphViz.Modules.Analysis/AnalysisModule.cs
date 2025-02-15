using Prism.Ioc;
using Prism.Modularity;
using Prism.Navigation.Regions;

namespace Plainion.GraphViz.Modules.Analysis
{
    public class AnalysisModule : IModule
    {
        private IRegionManager myRegionManager;

        public AnalysisModule(IRegionManager regionManager)
        {
            myRegionManager = regionManager;
        }

        public void RegisterTypes(IContainerRegistry containerRegistry)
        {
            myRegionManager.RegisterViewWithRegion(GraphViz.Infrastructure.RegionNames.SearchBox, typeof(SearchBox));
            myRegionManager.RegisterViewWithRegion(GraphViz.Infrastructure.RegionNames.NodeMasksEditor, typeof(NodeMasksEditor));
            myRegionManager.RegisterViewWithRegion(GraphViz.Infrastructure.RegionNames.NodeMasksView, typeof(NodeMasksView));
            myRegionManager.RegisterViewWithRegion(GraphViz.Infrastructure.RegionNames.ClusterEditor, typeof(ClusterEditor));
            myRegionManager.RegisterViewWithRegion(GraphViz.Infrastructure.RegionNames.Bookmarks, typeof(Bookmarks));
        }

        public void OnInitialized(IContainerProvider containerProvider)
        {
        }
    }
}
