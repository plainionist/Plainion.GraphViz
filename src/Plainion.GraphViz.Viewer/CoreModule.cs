using Plainion.GraphViz.Viewer.Views;
using Prism.Ioc;
using Prism.Modularity;
using Prism.Regions;

namespace Plainion.GraphViz.Viewer
{
    class CoreModule : IModule
    {
        private IRegionManager myRegionManager;

        public CoreModule(IRegionManager regionManager)
        {
            myRegionManager = regionManager;
        }

        public void RegisterTypes(IContainerRegistry containerRegistry)
        {
            myRegionManager.RegisterViewWithRegion(GraphViz.Viewer.RegionNames.GraphViewer, typeof(GraphViewer));
            myRegionManager.RegisterViewWithRegion(GraphViz.Viewer.RegionNames.SettingsEditor, typeof(SettingsEditor));
            myRegionManager.RegisterViewWithRegion(GraphViz.Viewer.RegionNames.StatusMessagesViewer, typeof(StatusMessagesView));
        }

        public void OnInitialized(IContainerProvider containerProvider)
        {
        }
    }
}
