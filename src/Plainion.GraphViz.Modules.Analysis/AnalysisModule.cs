using Plainion.GraphViz.Modules.Analysis.Bookmarks;
using Plainion.GraphViz.Modules.Analysis.Clusters;
using Plainion.GraphViz.Modules.Analysis.Filters;
using Plainion.GraphViz.Modules.Analysis.Search;
using Prism.Ioc;
using Prism.Modularity;
using Prism.Navigation.Regions;

namespace Plainion.GraphViz.Modules.Analysis;

public class AnalysisModule(IRegionManager regionManager) : IModule
{
    public void RegisterTypes(IContainerRegistry containerRegistry)
    {
        regionManager.RegisterViewWithRegion(Viewer.Abstractions.RegionNames.ToolBox_SearchBox, typeof(SearchBox));
        regionManager.RegisterViewWithRegion(Viewer.Abstractions.RegionNames.NodeMasksEditor, typeof(NodeMasksEditor));
        regionManager.RegisterViewWithRegion(RegionNames.NodeMasksView, typeof(NodeMasksView));
        regionManager.RegisterViewWithRegion(Viewer.Abstractions.RegionNames.ClusterEditor, typeof(ClusterEditor));
        regionManager.RegisterViewWithRegion(Viewer.Abstractions.RegionNames.Bookmarks, typeof(BookmarksView));
    }

    public void OnInitialized(IContainerProvider containerProvider)    {    }
}
