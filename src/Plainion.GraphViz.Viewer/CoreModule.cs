using System.ComponentModel.Composition;
using Plainion.GraphViz.Viewer.Views;
using Prism.Mef.Modularity;
using Prism.Modularity;
using Prism.Regions;

namespace Plainion.GraphViz.Viewer
{
    [ModuleExport( typeof( CoreModule ) )]
    public class CoreModule : IModule
    {
        [Import]
        public IRegionManager RegionManager { get; set; }

        public void Initialize()
        {
            RegionManager.RegisterViewWithRegion( GraphViz.Viewer.RegionNames.GraphViewer, typeof( GraphViewer ) );
            RegionManager.RegisterViewWithRegion( GraphViz.Viewer.RegionNames.SettingsEditor, typeof( SettingsEditor ) );
            RegionManager.RegisterViewWithRegion( GraphViz.Viewer.RegionNames.StatusMessagesViewer, typeof( StatusMessagesView ) );
        }
    }
}
