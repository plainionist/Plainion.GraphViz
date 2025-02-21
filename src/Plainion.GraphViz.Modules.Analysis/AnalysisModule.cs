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
            myRegionManager.RegisterViewWithRegion(Viewer.Abstractions.RegionNames.SearchBox, typeof(SearchBox));
            myRegionManager.RegisterViewWithRegion(Viewer.Abstractions.RegionNames.NodeMasksEditor, typeof(NodeMasksEditor));
            myRegionManager.RegisterViewWithRegion(Viewer.Abstractions.RegionNames.NodeMasksView, typeof(NodeMasksView));
            myRegionManager.RegisterViewWithRegion(Viewer.Abstractions.RegionNames.ClusterEditor, typeof(ClusterEditor));
            myRegionManager.RegisterViewWithRegion(Viewer.Abstractions.RegionNames.Bookmarks, typeof(Bookmarks));
        }

        public void OnInitialized(IContainerProvider containerProvider)
        {
        }
    }
}
