using System.ComponentModel.Composition;
using Microsoft.Practices.Prism.MefExtensions.Modularity;
using Microsoft.Practices.Prism.Modularity;
using Microsoft.Practices.Prism.Regions;

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
        }
    }
}
