using System.ComponentModel.Composition;
using Prism.Mef.Modularity;
using Prism.Modularity;
using Prism.Regions;

namespace Plainion.GraphViz.Modules.Analysis
{
    [ModuleExport( typeof( AnalysisModule ) )]
    public class AnalysisModule : IModule
    {
        [Import]
        public IRegionManager RegionManager { get; set; }

        public void Initialize()
        {
            RegionManager.RegisterViewWithRegion( GraphViz.Infrastructure.RegionNames.SearchBox, typeof( SearchBox ) );
            RegionManager.RegisterViewWithRegion( GraphViz.Infrastructure.RegionNames.NodeMasksEditor, typeof( NodeMasksEditor ) );
            RegionManager.RegisterViewWithRegion( GraphViz.Infrastructure.RegionNames.NodeMasksView, typeof( NodeMasksView ) );
            RegionManager.RegisterViewWithRegion( GraphViz.Infrastructure.RegionNames.ClusterEditor, typeof( ClusterEditor ) );
        }
    }
}
