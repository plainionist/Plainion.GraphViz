using System.ComponentModel.Composition;
using Plainion.GraphViz.Viewer.Views;
using Prism.Mef.Modularity;
using Prism.Modularity;
using Prism.Regions;

namespace Plainion.GraphViz.Viewer
{
    [ModuleExport( typeof( CoreModule ) )]
    class CoreModule : IModule
    {
        private IRegionManager myRegionManager;

        [ImportingConstructor]
        public CoreModule(IRegionManager regionManager)
        {
            myRegionManager = regionManager;
        }

        public void Initialize()
        {
            myRegionManager.RegisterViewWithRegion( GraphViz.Viewer.RegionNames.GraphViewer, typeof( GraphViewer ) );
            myRegionManager.RegisterViewWithRegion( GraphViz.Viewer.RegionNames.SettingsEditor, typeof( SettingsEditor ) );
            myRegionManager.RegisterViewWithRegion( GraphViz.Viewer.RegionNames.StatusMessagesViewer, typeof( StatusMessagesView ) );
        }
    }
}
