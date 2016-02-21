using System.ComponentModel.Composition;
using Microsoft.Practices.Prism.MefExtensions.Modularity;
using Microsoft.Practices.Prism.Modularity;
using Microsoft.Practices.Prism.Regions;
using Plainion.GraphViz.Modules.Reflection.Analysis.Inheritance;

namespace Plainion.GraphViz.Modules.Reflection
{
    [ModuleExport( typeof( ReflectionModule ) )]
    public class ReflectionModule : IModule
    {
        [Import]
        public IRegionManager RegionManager { get; set; }

        public void Initialize()
        {
            RegionManager.RegisterViewWithRegion( Infrastructure.RegionNames.AddIns, typeof( InheritanceGraphMenuItem ) );
            RegionManager.RegisterViewWithRegion( RegionNames.InheritanceGraphBuilder, typeof( InheritanceGraphBuilderView ) );
        }
    }
}
